using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text.RegularExpressions;


namespace CWLU
{
    public partial class Launcher : Form
    {

        #region Globális változók

        enum mentes_nev
        {
            program_verzio,
            forditas_verzio,
            auto_program,
            auto_nyelv,
            nev,
            auto_nev,
            cwmappa
        };

        string[] mentes_Mezok = new string[] {
            "ProgramVersion",              //0
            "ForditasVersion",             //1
            "AutoProgram",                 //2
            "AutoNyelv",                   //3
            "Nev",                         //4
            "AutoNev",                     //5
            "CWMappa"                      //6
        };

        List<string> mentes_Ertekek;
        List<string> mentes_alap_Ertekek;

        const string fileNev = "CWHUpdate.ms";
        const string host = "cwh.no-ip.hu";

        const string cubeExe = "cube.exe";
        const string cubeLauncher = "CubeLauncher.exe";

        const string delBat = "del.bat";
        const string runBat = "run.bat";

        string http_host;

        bool Online = false;

        #endregion

        #region Függvények

        public Launcher()
        {
            InitializeComponent();

            //Ez nem kell mert a Bat maga fogja törölni magát ha bezárod a Launchert - 2013_07_26_1620
            //FajlTorles(delBat);

            http_host = "http://" + host + "/";
            MentesEpites();
            mentes_alap_Ertekek = mentes_Ertekek;
        }


        void Indul(object sender, EventArgs e)
        {
            //Változók létrehozása
            bool BeallitasokFajl = false;
            string BeallitasokFajlHely = fileNev;


            #region Szerver elérhetőség ellenőrzése
            TcpClient tcpClient = new TcpClient();
            IAsyncResult ar = tcpClient.BeginConnect(host, 80, null, null);
            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), false))
                {
                    tcpClient.Close();
                    throw new TimeoutException();
                }
                else
                    Online = true;

                //Ha elérhető, akkor adatok feltöltése
                if (Online == true)
                {

                    //Hírek betöltse
                    string Hirek = new System.Net.WebClient().DownloadString(http_host + "hirek.ms");
                    HirekTxt.Text = Hirek;

                    //OnlineOffline.Text = "Elérhető";
                    string OnlineBeallitasok = new System.Net.WebClient().DownloadString(http_host + "forditasversion.ms");

                    string[] OnlineSorok = OnlineBeallitasok.Split(';');
                    string[] OnlineProgram = OnlineSorok[0].Split('=');
                    string[] OnlineNyelv = OnlineSorok[1].Split('=');
                    OnlineProgramVerzio.Text = OnlineProgram[1];
                    OnlineNyelvVerzio.Text = OnlineNyelv[1];
                }
                //else OnlineOffline.Text = "Nem Elérhető";
            }
            catch (Exception)
            {
                //OnlineOffline.Text = "Nem elérhető";
                Online = false;
            }
            #endregion

            #region Beállítások létrehozása és betöltése
            BeallitasokFajl = (File.Exists(BeallitasokFajlHely) ? true : false);
            if (!BeallitasokFajl)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(BeallitasokFajlHely, true))
                    {
                        writer.Write(AlapBeallitasErekek(mentes_Ertekek));
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Mentesek betöltése.", "Hiba");
                }

                // MessageBox.Show("Üdvözöllek!\nMiszter Soul vagyok, és ez itt az első verziója a Cube World Hungary - Frissítő programjának, ami automatikusan felajánlja a legújabban megjelent frissítésem letöltését a játékhoz.\nAhhoz, hogy ez működjön, csak ennyit kell tenned.\n1. Tedd a programot a Cube World mappájába,\n2. Indítsd el.\n3. A program első alkalommal mindenféle képpen felajánlja, hogy letöltsd a magyarítást, hisz a legfrissebbet tölti le, nem tudhatja, hogy milyen van a gépeden.\n\nA program kiírja, a szerver állapotát, és a fordítás verzióját, ha frissítés lesz elérhető a programhoz, azt jelezni fogja, és le fogja tölteni (írni fogja ha kell tenned valamit utána)\n\nJó játékot kívánok!\nMiszter Soul\n\nHa tetszik a program / fordítás, akkor a munkámat annyival értékeld, hogy nyomj egy like-ot a Cube World HU facebook oldalán, és egy feliratkozást a csatornámra. (Linkek a program alsó részén)");
            }
            else
            {

                //Van beállítás fálj szóval be lehet állítani a Form elemeinek értékeit a mentés alapján
                string[] Sorok = File.ReadAllText(fileNev).Split(';');

                string[] OfflineProgramVersion = Sorok[0].Split('=');
                string[] OfflineNyelvVersion = Sorok[1].Split('=');
                string[] OfflineAutoProgram = Sorok[2].Split('=');
                string[] OfflineAutoNyelv = Sorok[3].Split('=');
                string[] OfflineNev = Sorok[4].Split('=');
                string[] OfflineAutoNev = Sorok[5].Split('=');
                
                


                
                string[] CWMappahely = Sorok[6].Split('=');
                CWMappa.Text = CWMappahely[1].Replace("/", "\\");
                
                //Offline program verzió
                OfflineProgramVerzio.Text = OfflineProgramVersion[1];
                
                //Offline nyelv verzió
                OfflineNyelvVerzio.Text = OfflineNyelvVersion[1];

                bool notOnline = false;

                //Automatikus Nyelv Pipa-e
                if (OfflineAutoNyelv[1] == "True")
                {
                    AutoNyelv.Checked = true;
                    NyelvFrissites.Enabled = false;

                    if (CWMappa.Text != "None")
                        if (Online)
                            NyelvemFrissites();
                        else
                        {
                            notOnline = true;
                            MessageBox.Show("A server nem elérhető. Próbáld késöbb...", "Hiba");
                        }
                }
                else
                    AutoNyelv.Checked = false;


                //Automatikus Program(Launcher)
                if (OfflineAutoProgram[1] == "True")
                {
                    AutoProgram.Checked = true;
                    ProgramFrissites.Enabled = false;
                   
                    if (int.Parse(OnlineProgramVerzio.Text) > int.Parse(OfflineProgramVerzio.Text))
                        if (CWMappa.Text != "None")
                            if(Online)
                                ProgramomFrissites();
                            else if (!notOnline)
                            {
                                notOnline = true;
                                MessageBox.Show("A server nem elérhető. Próbáld késöbb...", "Hiba");
                            }
                }
                else
                    AutoProgram.Checked = false;


                //AutoNév Pipa-e
                if (OfflineAutoNev[1] == "True")
                {
                    AutoNev.Checked = true;
                    NevText.Text = OfflineNev[1];
                }
                else
                    AutoNev.Checked = false;
            }


            #endregion

            if(int.Parse(OnlineProgramVerzio.Text) > int.Parse(OfflineProgramVerzio.Text))
                ProgramFrissites.Enabled = true;

            if(int.Parse(OnlineNyelvVerzio.Text) > int.Parse(OfflineNyelvVerzio.Text))
                NyelvFrissites.Enabled = true;

        }

        //Telepítés helye
        void CWTelepites(object sender, EventArgs e)
        {
            var CubeFolder = new System.Windows.Forms.FolderBrowserDialog();

            if (CubeFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(CubeFolder.SelectedPath, cubeExe, SearchOption.AllDirectories);
                if (files.Length >= 1)
                {
                    CWMappa.Text = CubeFolder.SelectedPath;

                    //Újra építi a mentés listát mert megválltozik a CWMappa elérési útja
                    MentesEpites();

                    //AutoNyelv - 2013_07_26_1619
                    if (mentes_Ertekek[1] == "0")
                    {
                        NyelvFrissites.Enabled = true;
                    }

                }
                else
                    MessageBox.Show("A " + cubeExe + " nem található ebben a mappában amit kiválasztottál: " + CubeFolder.SelectedPath, "HIBA");
            }
        }

        //Automatikus Nyelv frissítő
        void NyelvemFrissites()
        {
            using (WebClient Client = new WebClient())
                try
                {
                    Client.DownloadFile(http_host + "data4.ms", CWMappa.Text + "\\data4.db");
                    Client.DownloadFile(http_host + "Olvassel.ms", CWMappa.Text + "\\OlvassEl.txt");
                    OfflineNyelvVerzio.Text = OnlineNyelvVerzio.Text;

                    //Újra kell építeni a mentés listát mert megválltozott az OfflineNyelvVerzio
                    MentesEpites();

                    NyelvFrissites.Enabled = false;
                }
                catch (Exception)
                {
                    MessageBox.Show("Válaszd ki a játék telepítésének a helyét.", "Hiba");
                }

        }

        /*Automatikus Program(Launcher) Frissítő
            Letölti az új verziót
            Bezárja a régi Launchert
            Átnevezi az új Lauchert és elindtja
        */
        void ProgramomFrissites()
        {
            using (WebClient Client = new WebClient())
                try
                {
                    Client.DownloadFile(http_host + "cwhu.exe", "Cube World Hungary Launcher v" + OnlineProgramVerzio.Text + ".exe");
                    OfflineProgramVerzio.Text = OnlineProgramVerzio.Text;

                    //Újra kell építeni a mentést mert megváltozott a OfflineProgramVerzio
                    BeallitasMentes(true, false);

                    ProgramFrissites.Enabled = false;

                    FajlTorles(delBat);
                    using (StreamWriter writer = new StreamWriter(delBat, true))
                    {
                        string regiNev = Application.StartupPath + "\\" + System.AppDomain.CurrentDomain.FriendlyName;//.Replace(".exe", OnlineProgramVerzio.Text + ".exe");

                        //régi rossz frissitve 2013_07_26_1316
                        //writer.Write("del \"" + regiNev.Replace(".exe", OnlineProgramVerzio.Text + ".exe") + "\"" +
                        //    "\nren \"" + "\\" + regiNev + "\" \"" + System.AppDomain.CurrentDomain.FriendlyName + "\"");

                        writer.WriteLine("@ECHO OFF");
                        writer.WriteLine("timeout /t 1");
                        writer.WriteLine("del \"" + regiNev.Replace(".exe", " v" + OnlineProgramVerzio.Text + ".exe") + "\"");
                        writer.WriteLine("ren \"" + regiNev + "\" \"" + System.AppDomain.CurrentDomain.FriendlyName + "\"");

                        writer.WriteLine("timeout /t 1");

                        //MessageBox.Show(System.AppDomain.CurrentDomain.FriendlyName);

                        writer.WriteLine("\"" + System.AppDomain.CurrentDomain.FriendlyName + "\"");
                        writer.WriteLine("del %0");
                        writer.WriteLine("exit");
                    }

                    #region Csak Command
                    /*
                string tempNev = Application.StartupPath + "\\" + System.AppDomain.CurrentDomain.FriendlyName;//.Replace(".exe", OnlineProgramVerzio.Text + ".exe");
                    
                tempNev = System.AppDomain.CurrentDomain.FriendlyName;


                string updateCommand = "@ECHO OFF\n" +
                    "timeout /t 1\n" +
                    "del \"" + tempNev.Replace(".exe", " v" + OnlineProgramVerzio.Text + ".exe") + "\"\n" +
                    "ren \"" + tempNev + "\" \"" + System.AppDomain.CurrentDomain.FriendlyName + "\"\npause";

                ExecuteCommand(updateCommand);
*/
                    #endregion


                    Process gocube = null;
                    try
                    {
                        string cubedir = string.Format(Application.StartupPath);
                        gocube = new Process();
                        gocube.StartInfo.WorkingDirectory = cubedir;
                        gocube.StartInfo.FileName = delBat;

                        gocube.StartInfo.CreateNoWindow = true;
                        gocube.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                        gocube.Start();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("A " + delBat + " fájl nem indítható el.", "Hiba");
                    }


                    //FONTOS! -> OnlineProgramVerzio mentés
                    if (File.Exists(fileNev))
                    {
                        string[] Sorok = File.ReadAllText(fileNev).Split(';');
                        File.WriteAllText(fileNev, Regex.Replace(File.ReadAllText(fileNev), Sorok[0], mentes_Mezok[0] + "=" + OnlineProgramVerzio.Text));

                        this.Close();
                        //MessageBox.Show("Frissites történt!");
                    }

                }
                catch (Exception)
                {
                    MessageBox.Show("Program frissítés", "Hiba");
                }
        }


        //Beállítások mentése
        //rebuild -> a biztonság kedvéért mégegyszer elkéri a form adatait
        //popup   ->ha megtörtént a mentés akkor kell-e felugró ablak
        public void BeallitasMentes(bool rebuild=false, bool popup=false)
        {
            //nem feltétlenül kell de fő a biztonság 
            if(rebuild)
                MentesEpites();
            
            FajlTorles(fileNev);

            try
            {
                using (StreamWriter writer = new StreamWriter(fileNev, true))
                {
                    for (int i = 0; i < mentes_Mezok.Length; i++)
                    {
                        writer.WriteLine(mentes_Mezok[i] + "=" + mentes_Ertekek[i] + ";");
                    }
                    writer.Close();

                    if (popup)
                        MessageBox.Show("A beállítások mentése elkészült.", "Siker");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Mentés probléma", "Hiba");
            }
        }


        #region Xii függvények

        string AlapBeallitasErekek(List<string> ertetek=null)
        {
            if (ertetek == null)
                ertetek = mentes_alap_Ertekek;

            string alaptartalom = "";
            for (int i = 0; i < mentes_Mezok.Length; i++)
            {
                alaptartalom += mentes_Mezok[i] + "=" + ertetek[i] + ";";
                
                //csak akkor tesz új sort ha szükséges
                if (i + 1 < mentes_Mezok.Length)
                    alaptartalom += "\n";
            }

            return alaptartalom;
        }

        void MentesEpites()
        {
            //Ezt sajnos manuálisan kell hozzáadni ha újabb értéket szeretnél eltárolni, az indexek mellékelve

            mentes_Ertekek = new List<string>(mentes_Mezok.Length);
            mentes_Ertekek.Clear();

            mentes_Ertekek.Add(OfflineProgramVerzio.Text);           //0
            mentes_Ertekek.Add(OfflineNyelvVerzio.Text);             //1
            mentes_Ertekek.Add(AutoProgram.Checked.ToString());      //2
            mentes_Ertekek.Add(AutoNyelv.Checked.ToString());        //3
            mentes_Ertekek.Add(NevText.Text);                        //4
            mentes_Ertekek.Add(AutoNev.Checked.ToString());          //5
            mentes_Ertekek.Add(CWMappa.Text);                        //6
        }

        void BatchIndito(string futtatniKivantFajl)
        {
            if (CWMappa.Text != "None")
            {
                using (StreamWriter writer = new StreamWriter(CWMappa.Text + "/" + runBat, false))
                {
                    writer.WriteLine("@ECHO OFF");
                    writer.WriteLine("start " + futtatniKivantFajl);
                }

                Process gocube = null;
                try
                {
                    string cubedir = string.Format(CWMappa.Text);
                    gocube = new Process();
                    gocube.StartInfo.WorkingDirectory = cubedir;
                    gocube.StartInfo.FileName = runBat;

                    gocube.StartInfo.CreateNoWindow = true;
                    gocube.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    gocube.Start();
                }
                catch (Exception)
                {
                    MessageBox.Show(runBat + " vagy " + futtatniKivantFajl + " nem található.", "Hiba");
                }

                this.Close();
            }
            else
                MessageBox.Show("Nem adtad meg a játék telepítési helyét.", "Hiba");
                    // Az indításhoz add meg a játék telepítésének helyét a beállításoknál.");
            
        }

        void FajlTorles(string faljNev)
        {
            if (File.Exists(faljNev))
                File.Delete(faljNev);
        }


        //Be kellene állítani hogy működjön de nem biztos hogy ez a legjobb megoldás rá
        public void ExecuteCommand(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe")
            {
                UseShellExecute = false,
                RedirectStandardInput = true
            };
            Process proc = new Process() { StartInfo = psi };

            proc.Start();

            proc.StandardInput.Write(command);

            proc.WaitForExit();
            //proc.Close();
        }

        #endregion


        #region Gombok
        //link_Facebook gomb
        void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://facebook.com/CubeWorldHU");
        }

        //link_Cube World gomb
        void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://picroma.com/CubeWorld");
        }

        //link_Youtube feliratkozás gomb
        void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.youtube.com/channel/UCOH3y9lWJaH2gnHDyPgtRIA?sub_confirmation=1");
        }

        //link_Nyereményjáték gomb
        void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.facebook.com/eboxpc");
            Process.Start("https://www.facebook.com/mrSoul.Gameplays/posts/661801770515785");
        }

        //link_Saját weboldal gomb
        void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(http_host);
        }


        //Bejelentkező gomb
        void Belepes_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Ez a funkció még nem elérhető.", "Hiba");
        }


        //Nyelv frissítés gomb
        void NyelvFrissites_Click(object sender, EventArgs e)
        {
            NyelvemFrissites();

            if (OnlineNyelvVerzio.Text == OfflineNyelvVerzio.Text)
            {
                BeallitasMentes(true, false);
            }


/*
            //Mentes file-ba
            if (OnlineNyelvVerzio.Text == OfflineNyelvVerzio.Text)
            {
                if (File.Exists(fileNev))
                {
                    string[] Sorok = File.ReadAllText(fileNev).Split(';');
                    File.WriteAllText(fileNev, Regex.Replace(File.ReadAllText(fileNev), Sorok[1], "\nForditasVersion=" + OfflineNyelvVerzio.Text));
                }
            }
*/
        }

        //Program frissítés gomb
        void ProgramFrissites_Click(object sender, EventArgs e)
        {
            ProgramomFrissites();
        }

        //gomb
        void BeallitasMentes_Click(object sender, EventArgs e)
        {
            BeallitasMentes(false, true);
        }



        //Launcher indító gomb
        void LauncherInditas(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Biztosan elindítod a Launchert?\n(A "+ cubeLauncher.Replace(".exe", "") +" lefrissíti a játékot, és felülírja a magyarítást)", "CW Launcher indítása?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                OfflineNyelvVerzio.Text = "0";
                BatchIndito(cubeLauncher);
            }
        }

        //CubeWorld indito gomb
        void CubeWorldInditas(object sender, EventArgs e)
        {
            BatchIndito(cubeExe);
        }


        //Amikor bezárja az ablakot
        void DelBat(object sender, FormClosingEventArgs e)
        {

            //Ez a sor azért kell, hogy mielőtt kitörölné a run.bat fálj előtte azért hagyjuk hogy elinduljon a run.bat! :)
            System.Threading.Thread.Sleep(200);

            FajlTorles(CWMappa.Text + "/" + runBat);
            FajlTorles(CWMappa.Text + "/" + delBat);


/*          
            //Régi verzió Frissitve 2013_07_26_1357
            bool batexist = false;
            batexist = (File.Exists(CWMappa.Text + "/" + runBat) ? true : false);
            if(batexist)
            {
                File.Delete(CWMappa.Text + "/" + runBat);
            }
           
            batexist = (File.Exists(CWMappa.Text + "/" + delBat) ? true : false);
            if(batexist)
            {
                File.Delete(CWMappa.Text + "/" +delBat);
            }
*/
        }

        #region ToolStripMenuItem
        void szerverElérhetőségToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Szerver elérhetőség ellenőrzése
            TcpClient tcpClient = new TcpClient();
            IAsyncResult ar = tcpClient.BeginConnect(host, 12345, null, null);
            
            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2), false))
                {
                    tcpClient.Close();
                    throw new TimeoutException();
                }
                MessageBox.Show("A szerver fut!\nCsatlakozz te is: " + host, "Állapot: Elérhető");
            }
            catch (Exception)
            {
                MessageBox.Show("A szerver NEM fut!\nNézz fel a honlapra, hátha van valami oka, hogy nem fut.\n" + http_host, "Állapot: Nem elérhető");
            }

        }

        void beküldömAzÖtletemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.facebook.com/messages/CubeWorldHU");
        }

        void beállításokTörléseToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (StreamWriter writer = new StreamWriter(fileNev))
            {
                writer.Write(AlapBeallitasErekek());
            }

            //Alapok betöltése
            string Beallitasok = File.ReadAllText(fileNev);
            string[] Sorok = Beallitasok.Split(';');

            string[] OfflineProgramVersion = Sorok[0].Split('=');
            string[] OfflineNyelvVersion   = Sorok[1].Split('=');
            string[] OfflineAutoProgram    = Sorok[2].Split('=');
            string[] OfflineAutoNyelv      = Sorok[3].Split('=');
            string[] OfflineNev            = Sorok[4].Split('=');
            string[] OfflineAutoNev        = Sorok[5].Split('=');
            string[] CWMappahely           = Sorok[6].Split('=');

            mentes_Ertekek = new List<string>(Sorok.Length);
            mentes_Ertekek.Clear();

/*
            for (int i = 0; i < Sorok.Length; i++)
            {
                
                string[] adat = Sorok[i].Split('=');
                MessageBox.Show(adat[1]);

                mentes_Ertekek.Add( adat[1] );
            }
*/

                CWMappa.Text = CWMappahely[1].Replace("/", "\\");
            
            //Offline program verzió
            OfflineProgramVerzio.Text = OfflineProgramVersion[1];
           
            //Offline nyelv verzió
            OfflineNyelvVerzio.Text = OfflineNyelvVersion[1];

            //AutoNyelv Pipa-e
            if (OfflineAutoNyelv[1].ToString() == "True")
            {
                AutoNyelv.Checked = true;
                NyelvFrissites.Enabled = false;
            }
            else
                AutoNyelv.Checked = false;
            
            //AutoProgram
            if (OfflineAutoProgram[1].ToString() == "True")
            {
                AutoProgram.Checked = true;
                ProgramFrissites.Enabled = false;
            }
            else
                AutoProgram.Checked = false;

            //AutoNév Pipa-e
            if (OfflineAutoNev[1].ToString() == "True")
            {
                AutoNev.Checked = true;
                NevText.Text = OfflineNev[1];
            }
            else
                AutoNev.Checked = false;

            MessageBox.Show("A beállítások törlésre kerültek.", "Siker");
        }
        #endregion


        #endregion

        #endregion

    }
}