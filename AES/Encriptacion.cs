using System;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace AES
{
    public partial class Encriptacion : Form
    {
        private string clave = "";

        public Encriptacion()
        {
            InitializeComponent();
        }
        private string PedirClave()
        {
            string clave = "";

            string input = Microsoft.VisualBasic.Interaction.InputBox("Ingrese la clave:", "Clave", "", -1, -1);
            if (!string.IsNullOrEmpty(input))
            {
                clave = input;
            }

            return clave;
        }

        private byte[] DerivarClave(string clave, int keySize)
        {
            byte[] keyBytes = new byte[keySize / 8];
            Array.Copy(Encoding.UTF8.GetBytes(clave), keyBytes, Math.Min(keyBytes.Length, clave.Length));
            return keyBytes;
        }

        private void btnEncriptar_Click(object sender, EventArgs e)
        {
            string mensaje = textBoxMensaje.Text;
            clave = PedirClave();
            if (clave != "")
            {
                byte[] mensajeEncriptado = Encriptar(mensaje, clave);
                textBoxEncriptado.Text = Convert.ToBase64String(mensajeEncriptado);
            }
            else
            {
                MessageBox.Show("No se ingresó una clave válida.");
            }
        }

        private void btnDesencriptar_Click(object sender, EventArgs e)
        {
            string mensajeEncriptadoString = textBoxEncriptado.Text;
            if (clave == "")
            {
                MessageBox.Show("Debes encriptar primero para ingresar la clave.");
                return;
            }

            string claveDesencriptar = PedirClave();
            if (claveDesencriptar != clave)
            {
                MessageBox.Show("La clave ingresada no coincide con la clave de encriptación.");
                return;
            }

            byte[] mensajeEncriptado = Convert.FromBase64String(mensajeEncriptadoString);
            string mensajeDesencriptado = Desencriptar(mensajeEncriptado, claveDesencriptar);
            MessageBox.Show("Mensaje desencriptado: " + mensajeDesencriptado);
        }

        private byte[] Encriptar(string mensaje, string clave)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = DerivarClave(clave, aesAlg.KeySize);
                aesAlg.GenerateIV();

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] bytesMensaje = Encoding.UTF8.GetBytes(mensaje);
                        csEncrypt.Write(bytesMensaje, 0, bytesMensaje.Length);
                    }

                    byte[] ivAndEncrypted = aesAlg.IV.Concat(msEncrypt.ToArray()).ToArray();
                    return ivAndEncrypted;
                }
            }
        }

        private string Desencriptar(byte[] mensajeEncriptado, string clave)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = DerivarClave(clave, aesAlg.KeySize);
                byte[] iv = mensajeEncriptado.Take(16).ToArray();
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(mensajeEncriptado.Skip(16).ToArray()))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
