﻿using System;
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

        enum m_nev
        {
            ver_launcher = 0,
            ver_forditas = 1,
            auto_program = 2,
            auto_nyelv = 3,
            nev = 4,
            auto_nev = 5,
            cwmappa = 6
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

        const string fajl_beallitas = "CWHUpdate.ms";
        const string host = "cwh.no-ip.hu";

        const string exe_cube = "cube.exe";
        const string exe_launcher = "CubeLauncher.exe";

        const string batch_del = "del.bat";
        const string batch_run = "run.bat";

        const int port_web = 80;
        const int port_cubeworld = 12345;

        //Online Adatok
        int ver_online_nyelv = -1;
        int ver_online_program = -1;

        List<string> mentes_Ertekek;
        List<string> mentes_alap_Ertekek;

        string http_host;

        bool online_game = false;
        bool online_web = false;

        #endregion

        #region Függvények

        public Launcher()
        {
            InitializeComponent();

            //Ez nem kell mert a Bat maga fogja törölni magát ha bezárod a Launchert - 2013_07_26_1620
            FajlTorles(batch_del);

            http_host = "http://" + host + "/";
            MentesEpites();
            mentes_alap_Ertekek = mentes_Ertekek;
        }

        //2013_07_27_1630
        void Indul(object sender, EventArgs e)
        {

            #region Szerver elérhetőség ellenőrzése

            //Új Server elérhetőség
            online_web = ServerPortEllenorzes(port_web);
            online_game = ServerPortEllenorzes(port_cubeworld);

            if (online_web)
            {
                HirekTxt.Text = new System.Net.WebClient().DownloadString(http_host + "hirek.ms");

                //Verziók Letöltse - 2013_07_27_1448
                string[] OnlineSorok = new System.Net.WebClient().DownloadString(http_host + "forditasversion.ms").Split(';');

                string[] OnlineProgram = OnlineSorok[0].Split('=');
                string[] OnlineNyelv = OnlineSorok[1].Split('=');

                OnlineProgramVerzio.Text = OnlineProgram[1];
                OnlineNyelvVerzio.Text = OnlineNyelv[1];
            }
            else
            {
                string web = online_web ? "Elérhető" : "Nem Elérhető";
                string game = online_game ? "Elérhető" : "Nem Elérhető";
                MessageBox.Show("Web oldal állapot:  " + web + "\nServer állapot:  " + game, "Állapot");
            }

            #region Régi Server elérhetőség check
            /*          
            //Régi - 2013_07_27_1548
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
                if (Online)
                {
                    //Hírek Letöltse - 2013_07_27_1433
                    //string Hirek = new System.Net.WebClient().DownloadString(http_host + "hirek.ms");
                    //HirekTxt.Text = Hirek;
                    HirekTxt.Text = new System.Net.WebClient().DownloadString(http_host + "hirek.ms");

                    //OnlineOffline.Text = "Elérhető";
                    //string[] OnlineBeallitasok = new System.Net.WebClient().DownloadString(http_host + "forditasversion.ms").Split(';');

                    //string[] OnlineSorok = OnlineBeallitasok.Split(';');

                    //Verziók Letöltse - 2013_07_27_1448
                    string[] OnlineSorok = new System.Net.WebClient().DownloadString(http_host + "forditasversion.ms").Split(';');

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
*/
            #endregion

            #endregion


            #region Beállítások létrehozása és betöltése

            if (!File.Exists(fajl_beallitas))
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(fajl_beallitas, true))
                    {
                        //Ekkor még a 'mentes_Ertekek' változóban az alapértékek vannak^^
                        writer.Write(AlapBeallitasErekek(mentes_Ertekek));
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Mentesek betöltése.", "Beállítás Hiba");
                }

                // MessageBox.Show("Üdvözöllek!\nMiszter Soul vagyok, és ez itt az első verziója a Cube World Hungary - Frissítő programjának, ami automatikusan felajánlja a legújabban megjelent frissítésem letöltését a játékhoz.\nAhhoz, hogy ez működjön, csak ennyit kell tenned.\n1. Tedd a programot a Cube World mappájába,\n2. Indítsd el.\n3. A program első alkalommal mindenféle képpen felajánlja, hogy letöltsd a magyarítást, hisz a legfrissebbet tölti le, nem tudhatja, hogy milyen van a gépeden.\n\nA program kiírja, a szerver állapotát, és a fordítás verzióját, ha frissítés lesz elérhető a programhoz, azt jelezni fogja, és le fogja tölteni (írni fogja ha kell tenned valamit utána)\n\nJó játékot kívánok!\nMiszter Soul\n\nHa tetszik a program / fordítás, akkor a munkámat annyival értékeld, hogy nyomj egy like-ot a Cube World HU facebook oldalán, és egy feliratkozást a csatornámra. (Linkek a program alsó részén)");
            }
            else
            {

                //Van beállítás fálj szóval be lehet állítani a Form elemeinek értékeit a mentés alapján
/*
                string[] Sorok = File.ReadAllText(fajl_beallitas).Split(';');

                string[] OfflineProgramVersion = Sorok[0].Split('=');
                string[] OfflineNyelvVersion = Sorok[1].Split('=');
                string[] OfflineAutoProgram = Sorok[2].Split('=');
                string[] OfflineAutoNyelv = Sorok[3].Split('=');
                string[] OfflineNev = Sorok[4].Split('=');
                string[] OfflineAutoNev = Sorok[5].Split('=');
                string[] CWMappahely = Sorok[6].Split('=');
                CWMappa.Text = CWMappahely[1].Replace("/", "\\");
*/

                string[] Sorok = File.ReadAllText(fajl_beallitas).Split(';');
                MentesEpites(Sorok);

                //Offline program verzió
                //OfflineProgramVerzio.Text = OfflineProgramVersion[1];
                OfflineProgramVerzio.Text = mentes_Ertekek[(int)m_nev.ver_launcher];

                //Offline nyelv verzió
                //OfflineNyelvVerzio.Text = OfflineNyelvVersion[1];
                OfflineNyelvVerzio.Text = mentes_Ertekek[(int)m_nev.ver_forditas];

                //Cube World Mappa elérési útja^^
                CWMappa.Text = mentes_Ertekek[(int)m_nev.cwmappa].Replace("/", "\\");

                //Néhány helyi válltozó a szép kinézethez :)
                bool update_lang = false;
                bool update_launcher = false;

                //Automatikus Nyelv
                //if (OfflineAutoNyelv[1] == "True")
                if (mentes_Ertekek[(int)m_nev.auto_nyelv] == "True")
                {
                    //Checkbox
                    AutoNyelv.Checked = true;
                    //Button
                    NyelvFrissites.Enabled = false;

                    //if (CWMappa.Text != "None")
                    if (mentes_Ertekek[(int)m_nev.cwmappa] != "None")
                        
                        if (online_web)
                            update_lang = true;
                }
                else
                    AutoNyelv.Checked = false;


                //Automatikus Program(Launcher)
                //if (OfflineAutoProgram[1] == "True")
                if(mentes_Ertekek[(int)m_nev.auto_program] == "True")
                {
                    //Checkbox
                    AutoProgram.Checked = true;
                    //Button
                    ProgramFrissites.Enabled = false;

                    if (int.Parse(OnlineProgramVerzio.Text) > int.Parse(OfflineProgramVerzio.Text))
                    {
                        ProgramFrissites.Enabled = true;

                        //if (CWMappa.Text != "None")
                        if (mentes_Ertekek[(int)m_nev.cwmappa] != "None")
                            if (online_web)
                                update_launcher = true;
                    }
                }
                else
                    AutoProgram.Checked = false;


                //AutoNév Pipa-e
                //if (OfflineAutoNev[1] == "True")
                if(mentes_Ertekek[(int)m_nev.auto_nev] == "True")
                {
                    AutoNev.Checked = true;
                    //NevText.Text = OfflineNev[1];
                    NevText.Text = mentes_Ertekek[(int)m_nev.nev];
                }
                else
                    AutoNev.Checked = false;


                if (update_lang)
                    NyelvemFrissites();
                if (update_launcher)
                    ProgramomFrissites();
            }


            #endregion

            int temp_ver_offline;
            int.TryParse(OfflineProgramVerzio.Text, out temp_ver_offline);
            int ver_offline_program = temp_ver_offline;


            ver_online_program = int.Parse(OnlineProgramVerzio.Text);


            int temp_offline_nyelv;
            int.TryParse(OfflineNyelvVerzio.Text, out temp_offline_nyelv);
            int ver_offline_nyelv = temp_offline_nyelv;

            int temp_online_nyelv;
            int.TryParse(OnlineNyelvVerzio.Text, out temp_online_nyelv);
            ver_online_nyelv = temp_online_nyelv;


            //2013_07_27_1613 - Szóval biztos lesznek olyanok akik megkeresik a mentés fáljt és bele kontárkodnak 
            //akkor van probléma ha magasabb verziót írnak bele mint ami az OnlineVerzió x|
            bool anticheat = false;

            if (ver_online_program > ver_offline_program)
                ProgramFrissites.Enabled = true;
            else
            {
                OfflineProgramVerzio.Text = ver_online_program.ToString();
                anticheat = true;
            }

            if (ver_online_nyelv > ver_offline_nyelv)
                NyelvFrissites.Enabled = true;
            else
            {
                OfflineNyelvVerzio.Text = ver_online_nyelv.ToString();
                anticheat = true;
            }

            if(anticheat)
                BeallitasMentes(true, false);
        }

        //Telepítés helye - 2013_07_27_1645
        void CWTelepites(object sender, EventArgs e)
        {
            var CubeFolder = new System.Windows.Forms.FolderBrowserDialog();

            if (CubeFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(CubeFolder.SelectedPath, exe_cube, SearchOption.AllDirectories);
                if (files.Length >= 1)
                {
                    CWMappa.Text = CubeFolder.SelectedPath;

                    //Újra építi a mentés listát mert megválltozik a CWMappa elérési útja
                    MentesEpites();

                    string ver_offline_nyelv = mentes_Ertekek[(int)m_nev.ver_forditas];
                    
                    //AutoNyelv - 2013_07_26_1619 -> 2013_07_27_1619 - LOL :P ez nem lehet véletlen
                    if (ver_offline_nyelv == "0" || int.Parse(ver_offline_nyelv) < ver_online_nyelv)
                    {
                        //Button
                        NyelvFrissites.Enabled = true;
                    }

                }
                else
                    MessageBox.Show("A " + exe_cube + " nem található ebben a mappában amit kiválasztottál: " + CubeFolder.SelectedPath, "Cube World Telepítési hely Hiba");
            }
        }

        //Automatikus Nyelv frissítő - 2013_07_27_1650
        void NyelvemFrissites()
        {
            using (WebClient Client = new WebClient())
                try
                {
                    if (online_web)
                    {
                        Client.DownloadFile(http_host + "data4.ms", CWMappa.Text + "\\data4.db");
                        Client.DownloadFile(http_host + "Olvassel.ms", CWMappa.Text + "\\OlvassEl.txt");

                        OfflineNyelvVerzio.Text = OnlineNyelvVerzio.Text;

                        //Újra kell építeni a mentés listát mert megválltozott az OfflineNyelvVerzio
                        MentesEpites();

                        NyelvFrissites.Enabled = false;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Válaszd ki a játék telepítésének a helyét.", "Cube World Telepítési hely Hiba");
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
                    if (online_web)
                    {
                        Client.DownloadFile(http_host + "cwhu.exe", "Cube World Hungary Launcher v" + OnlineProgramVerzio.Text + ".exe");
                        
                        OfflineProgramVerzio.Text = OnlineProgramVerzio.Text;
                        ProgramFrissites.Enabled = false;
                        
                        //Újra kell építeni a mentést mert megváltozott a OfflineProgramVerzio
                        BeallitasMentes(true, false);

                        FajlTorles(batch_del);
                        using (StreamWriter writer = new StreamWriter(batch_del, true))
                        {
                            //string regiNev = Application.StartupPath + "\"" + System.AppDomain.CurrentDomain.FriendlyName;
                            string regiNev = System.AppDomain.CurrentDomain.FriendlyName;
                            //MessageBox.Show("regiNev Check\n" + regiNev);

                            //I was here

                            //régi rossz frissitve 2013_07_26_1316
                            //writer.Write("del \"" + regiNev.Replace(".exe", OnlineProgramVerzio.Text + ".exe") + "\"" +
                            //    "\nren \"" + "\\" + regiNev + "\" \"" + System.AppDomain.CurrentDomain.FriendlyName + "\"");

                            writer.WriteLine("@ECHO OFF");
                            writer.WriteLine("timeout /t 2");
                            
                            //string del = "del \"" + regiNev.Replace(".exe", " v" + OnlineProgramVerzio.Text + ".exe") + "\"";
                            string del = "del \"" + regiNev + "\"";

                            //MessageBox.Show("del check\n" + del);
                            writer.WriteLine(del);

                            
                            //string rename = "ren \"" + regiNev + "\" \"" + System.AppDomain.CurrentDomain.FriendlyName + "\"";
                            string rename = "ren \"" + regiNev.Replace(".exe", " v" + OnlineProgramVerzio.Text + ".exe") + "\" \"" + regiNev + "\"";
                            //MessageBox.Show("rename check\n" + rename);
                            writer.WriteLine(rename);


                            writer.WriteLine("timeout /t 2");

                            //MessageBox.Show(System.AppDomain.CurrentDomain.FriendlyName);

                            writer.WriteLine("\"" + regiNev + "\"");
                            writer.WriteLine("del %0");
                            writer.WriteLine("exit");

                            writer.Close();
                        }

                        #region Csak Command, késöbb lesz...
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
                            gocube.StartInfo.FileName = batch_del;

                            //elrejti az ablakot
                            gocube.StartInfo.CreateNoWindow = true;
                            gocube.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                            gocube.Start();
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("A " + batch_del + " fájl nem található vagy indítható el.", "Hiba");
                        }


                        //FONTOS! -> OnlineProgramVerzio mentés -> OfflineVerzióvá
                        if (File.Exists(fajl_beallitas))
                        {
                            string teljesSzoveg = File.ReadAllText(fajl_beallitas);
                            string[] Sorok = teljesSzoveg.Split(';');

                            int verzio = (int)m_nev.ver_launcher;
                            File.WriteAllText(fajl_beallitas, Regex.Replace(teljesSzoveg, Sorok[verzio], mentes_Mezok[verzio] + "=" + OnlineProgramVerzio.Text));
                            //I was here 2013_07_27_1733

                            this.Close();
                            //MessageBox.Show("Frissites történt!");
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Launcher frissítés", "Hiba");
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
            
            FajlTorles(fajl_beallitas);
            try
            {
                using (StreamWriter writer = new StreamWriter(fajl_beallitas, true))
                {
                    for (int i = 0; i < mentes_Mezok.Length; i++)
                    {
                        writer.WriteLine(mentes_Mezok[i] + "=" + mentes_Ertekek[i] + ";");
                    }
                    writer.Close();

                    if (popup)
                        MessageBox.Show("A beállítások mentése elkészült.", "Beállítás Sikerült");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Mentés probléma", "Beállítás Hiba");
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

        void MentesEpites(string[] tomb = null)
        {
            mentes_Ertekek = new List<string>(mentes_Mezok.Length);
            mentes_Ertekek.Clear();
            
            if (tomb == null)
            {
                //Ezt sajnos manuálisan kell hozzáadni ha újabb értéket szeretnél eltárolni, az indexek mellékelve

                mentes_Ertekek.Add(OfflineProgramVerzio.Text);           //0
                mentes_Ertekek.Add(OfflineNyelvVerzio.Text);             //1
                mentes_Ertekek.Add(AutoProgram.Checked.ToString());      //2
                mentes_Ertekek.Add(AutoNyelv.Checked.ToString());        //3
                mentes_Ertekek.Add(NevText.Text);                        //4
                mentes_Ertekek.Add(AutoNev.Checked.ToString());          //5
                mentes_Ertekek.Add(CWMappa.Text);                        //6
            }
            else
            {
                for (int i = 0; i < tomb.Length - 1; i++)
                {
                    string[] adat = tomb[i].Split('=');
                    adat[1] = adat[1].Replace("/", "\\");
                    //MessageBox.Show(adat[1]);
                    
                    mentes_Ertekek.Add(adat[1]);
                }
            }
        }

        void BatchIndito(string futtatniKivantFajl)
        {
            if (CWMappa.Text != "None")
            {
                using (StreamWriter writer = new StreamWriter(CWMappa.Text + "/" + batch_run, false))
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
                    gocube.StartInfo.FileName = batch_run;

                    gocube.StartInfo.CreateNoWindow = true;
                    gocube.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    gocube.Start();
                }
                catch (Exception)
                {
                    MessageBox.Show(batch_run + " vagy " + futtatniKivantFajl + " nem található.", "Hiba");
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

        bool ServerPortEllenorzes(int port, bool popup=false)
        {
            //Szerver elérhetőség ellenőrzése
            TcpClient tcpClient = new TcpClient();
            IAsyncResult ar = tcpClient.BeginConnect(host, port, null, null);

            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2), false))
                {
                    tcpClient.Close();
                    throw new TimeoutException();
                }
                if(popup)
                    MessageBox.Show("A szerver fut!\nCsatlakozz te is: " + host, "Állapot: Elérhető");
                
                return true;
            }
            catch (Exception)
            {
                if (popup)
                    MessageBox.Show("A szerver NEM fut!\nNézz fel a honlapra, hátha van valami oka, hogy nem fut.\n" + http_host, "Állapot: Nem elérhető");
               
                return false;
            }
        }


        //Ezt még át kell néznem, de véglegesen ez lenne a jó megoldás a Batch helyett. - 2013_07_27_1847
        void ExecuteCommand(string command)
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
            DialogResult dialogResult = MessageBox.Show("Biztosan elindítod a Launchert?\n(A "+ exe_launcher.Replace(".exe", "") +" lefrissíti a játékot, és felülírja a magyarítást)", "CW Launcher indítása?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                OfflineNyelvVerzio.Text = "0";
                BatchIndito(exe_launcher);
            }
        }

        //CubeWorld indito gomb
        void CubeWorldInditas(object sender, EventArgs e)
        {
            BatchIndito(exe_cube);
        }


        //Amikor bezárja az ablakot
        void DelBat(object sender, FormClosingEventArgs e)
        {

            //Ez a sor azért kell, hogy mielőtt kitörölné a run.bat fálj előtte azért hagyjuk hogy elinduljon a run.bat! :)
            System.Threading.Thread.Sleep(100);

            FajlTorles(CWMappa.Text + "/" + batch_run);
            FajlTorles(CWMappa.Text + "/" + batch_del);


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
            //2013_07_27_1544
            ServerPortEllenorzes(port_cubeworld, true);

/*          //Régi
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
*/

        }

        void beküldömAzÖtletemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.facebook.com/messages/CubeWorldHU");
        }

        void beállításokTörléseToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (StreamWriter writer = new StreamWriter(fajl_beallitas))
            {
                writer.Write(AlapBeallitasErekek());
            }


            /*  
                //Régi
                //Alapok betöltése - 2013_07_27_1511
                //string Beallitasok = File.ReadAllText(fileNev);
                //string[] Sorok = Beallitasok.Split(';');
                string[] OfflineProgramVersion = Sorok[0].Split('=');
                string[] OfflineNyelvVersion = Sorok[1].Split('=');
                string[] OfflineAutoProgram = Sorok[2].Split('=');
                string[] OfflineAutoNyelv = Sorok[3].Split('=');
                string[] OfflineNev = Sorok[4].Split('=');
                string[] OfflineAutoNev = Sorok[5].Split('=');
                string[] CWMappahely = Sorok[6].Split('=');
            */

            //Fontos! ezek az értékek a kliensben vannak eltárolva, és az Offline adatokat lehet velük vissza állítani!!
            string[] Sorok = File.ReadAllText(fajl_beallitas).Split(';');
            MentesEpites(Sorok);


             //CWMappa.Text = CWMappahely[1].Replace("/", "\\");
            CWMappa.Text = mentes_Ertekek[(int)m_nev.cwmappa].Replace("/", "\\");
            
            //Offline program verzió
            //OfflineProgramVerzio.Text = OfflineProgramVersion[1];
            OfflineProgramVerzio.Text = mentes_Ertekek[(int)m_nev.ver_launcher];
           
            //Offline nyelv verzió
            //OfflineNyelvVerzio.Text = OfflineNyelvVersion[1];
            OfflineNyelvVerzio.Text = mentes_Ertekek[(int)m_nev.ver_forditas];

            //AutoNyelv Pipa-e
            //if (OfflineAutoNyelv[1].ToString() == "True")
            if (mentes_Ertekek[(int)m_nev.auto_nyelv].ToString() == "True")
            {
                AutoNyelv.Checked = true;
                NyelvFrissites.Enabled = false;
            }
            else
                AutoNyelv.Checked = false;
            
            //AutoProgram
            //if (OfflineAutoProgram[1].ToString() == "True")
            if (mentes_Ertekek[(int)m_nev.auto_program].ToString() == "True")
            {
                AutoProgram.Checked = true;
                ProgramFrissites.Enabled = false;
            }
            else
                AutoProgram.Checked = false;

            //AutoNév Pipa-e
            //if (OfflineAutoNev[1].ToString() == "True")
            if (mentes_Ertekek[(int)m_nev.auto_nev].ToString() == "True")
            {
                AutoNev.Checked = true;
                //NevText.Text = OfflineNev[1];
                NevText.Text = mentes_Ertekek[(int)m_nev.nev];
            }
            else
                AutoNev.Checked = false;

            MessageBox.Show("A beállítások törlésre kerültek.", "Beállítások");
        }
        #endregion


        #endregion

        #endregion

    }
}