using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Tea;

namespace Tea
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btEncode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tbPassword.Text.Length == 0)
                {
                    MessageBox.Show("Введите ключ");
                    tbPassword.BorderBrush = Brushes.Red;
                }
                else if (tbSource.Text.Length == 0)
                {
                    MessageBox.Show("Введите текст");
                    tbSource.BorderBrush = Brushes.Red;
                }
                else
                {
                    tbEncoded.Text = AlgoTea.Encrypt(tbSource.Text, tbPassword.Text);
                    tbEncoded.Background = Brushes.White;
                    tbPassword.BorderBrush = Brushes.Silver;
                    tbSource.BorderBrush = Brushes.Silver;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка шифрации");
            }
        }

        private void btDecode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tbDecoded.Text = AlgoTea.Decrypt(tbEncoded.Text, tbPassword.Text);
                tbDecoded.Background = Brushes.White;
                tbEncoded.Background = Brushes.White;
                tbPassword.BorderBrush = Brushes.Silver;
                tbSource.BorderBrush = Brushes.Silver;
            }
            catch (Exception ex)
            {
               MessageBox.Show("Ошибка дешифрации");
            }
        }

        private void Generate_keys_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Inf.Text = "";
                Inf.Background = Brushes.White;
                uint[] key = AlgoTea.CreateKey(Encoding.Default.GetBytes(tbPassword.Text));
                char[] charArray = tbPassword.Text.ToCharArray();
                Inf.Text = "Алгоритм шифрования TEA имеет 128-битный ключ шифрования. \nКлюч: ";
                for (int index = 0; index < charArray.Length; ++index)
                {
                    Inf.Text += charArray[index].ToString() + "      ";
                }
                Inf.Text += "\n         ";
                for (int index = 0; index < charArray.Length; ++index)
                {
                    Inf.Text += ((int)charArray[index]).ToString() + "  ";
                }
                
                Inf.Text += "\n\nКлюч делится на четыре 32-битных подключа: \n\nK[0] = " + Convert.ToString(key[0], 2) + " \nK[1] = " + Convert.ToString(key[1], 2) + " \nK[2] = " + Convert.ToString(key[2], 2) + " \nK[3] = " + Convert.ToString(key[3], 2);
                Inf.Text += "\n\nКаждый ключ в TEA эквивалентен трем другим, что означает, что эффективная длина ключа составляет 126 бит вместо 128.";
            }
            catch
            {
                MessageBox.Show("Ошибка");
            }
        }

        private void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Inf.Text = "";
                Inf.Background = Brushes.White;
                uint[] key = AlgoTea.CreateKey(Encoding.Default.GetBytes(tbPassword.Text));
                byte[] bytes1 = Encoding.Default.GetBytes(tbSource.Text);
                uint[] v = new uint[2];
                byte[] numArray = new byte[(bytes1.Length + 11) / 8 * 8];
                byte[] bytes2 = BitConverter.GetBytes(bytes1.Length);
                Array.Copy((Array)bytes2, (Array)numArray, bytes2.Length);
                Array.Copy((Array)bytes1, 0, (Array)numArray, bytes2.Length, bytes1.Length);
                Inf.Text = "Исходный текст разбивается на блоки по 64 бита каждый. Ключ делится на четыре 32-битных подключа: \n\nK[0] = " + Convert.ToString((long)key[0], 2) + " \nK[1] = " + Convert.ToString((long)key[1], 2) + " \nK[2] = " + Convert.ToString((long)key[2], 2) + " \nK[3] = " + Convert.ToString((long)key[3], 2) + ". \n\nВ нечетных раундах используются ключи K[0] и K[1], в четных - K[2] и K[3]. Каждый 64-битный блок шифруется на протяжении 32 циклов по нижеприведенному алгоритму.\n\n";
                Inf.Text += "На вход раунда поступают правая и левая часть блока (L, R) по 32 бита каждая:";
                using (MemoryStream memoryStream = new MemoryStream(numArray))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter((Stream)memoryStream))
                    {
                        v[0] = BitConverter.ToUInt32(numArray, 0);
                        v[1] = BitConverter.ToUInt32(numArray, 4);
                        Inf.Text += "\nL = " + Convert.ToString(v[0], 2) + "\nR = " + Convert.ToString(v[1], 2) + " ";
                        Inf.Text += "\nНа выходе будут левая и правая части (Ln+1, Rn+1), которые вычисляются по следующим правилам:\nЛевая часть: Ln+1 = Rn\nПравая часть зависит от четности раунда:\n   1)Если нечётные раунды: \n\tRn+1 = Ln + (( [ Rn <<  4 ] + K[0] ) xor ( Rn + i * δ ) xor ( [ Rn >> 5 ] + K[1] ))\n   2)Если чётные раунды: \n\tRn+1 = Ln + (( [ Rn <<  4 ] + K[2] ) xor ( Rn + i * δ ) xor ( [ Rn >> 5 ] + K[3] )).\nЗдесь:\n\t+ - операция сложения чисел по модулю 2^32\n\txor - побитовое исключающее «ИЛИ»\n\tX << Y и X >> Y — операции побитового сдвига числа X на Y бит влево и вправо соответственно\n\tКонстанта δ была выведена из Золотого сечения δ = 2654435769\nТаким образом выполняется 32 раунда\n";
                        Inf.Text += "\nНа выходе получаем первый зашифрованный блок сообщения:\n";
                        AlgoTea.BlockEncrypt(v, key);
                        binaryWriter.Write(v[0]);
                        binaryWriter.Write(v[1]);
                        Inf.Text += "\t1)  " + Encoding.Default.GetString(numArray).Trim().Substring(0, 8) + "\nОставшиеся зашифрованные блоки: \n";
                        for (int startIndex1 = 8, startIndex2 = 8, num = 1; startIndex1 < numArray.Length; startIndex1 += 8, startIndex2 += 8, num++)
                        {
                            v[0] = BitConverter.ToUInt32(numArray, startIndex1);
                            v[1] = BitConverter.ToUInt32(numArray, startIndex1 + 4);
                            AlgoTea.BlockEncrypt(v, key);
                            binaryWriter.Write(v[0]);
                            binaryWriter.Write(v[1]);
                            Inf.Text += $"\t{num})  {Encoding.Default.GetString(numArray).Trim().Substring(startIndex2, 8)}\n";

                        }
                    }
                    Inf.Text += "\nЗашифрованное сообщение целиком: " + Encoding.Default.GetString(numArray);
                }
            }
            catch {
                MessageBox.Show("Ошибка");
            }
        }

        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            try { 
            Inf.Text = "";
            Inf.Background = Brushes.White;
            Inf.Text = "Процесс дешифрации обратный процессу шифрации.\nРасшифрованное сообщение: " + Encoding.Default.GetString(AlgoTea.Decrypt(Encoding.Default.GetBytes(tbSource.Text), Encoding.Default.GetBytes(tbPassword.Text)));
            }
            catch {
                MessageBox.Show("Ошибка");
            }
        }

        private void tbPassword_TextChanged(object sender, RoutedEventArgs e)
        {
            if (tbPassword.Text.Length > 16)
            {
                MessageBox.Show("Длина ключа должна быть 16 символов!");
                tbPassword.Text = tbPassword.Text.Substring(0, 16);
                uint[] key = AlgoTea.CreateKey(Encoding.Default.GetBytes(tbPassword.Text));
                K0.Text = Convert.ToString(key[0], 2);
                K1.Text = Convert.ToString(key[1], 2);
                K2.Text = Convert.ToString(key[2], 2);
                K3.Text = Convert.ToString(key[3], 2);
                btEncode.IsEnabled = true;
                btDecode.IsEnabled = true;
                Encrypt.IsEnabled = true;
                Decrypt.IsEnabled = true;
                Generate_keys.IsEnabled = true;
                tbPassword.Background = Brushes.White;
                tbSource.Background = Brushes.White;
                K0.Background = Brushes.White;
                K1.Background = Brushes.White;
                K2.Background = Brushes.White;
                K3.Background = Brushes.White;
                tbPassword.BorderBrush = Brushes.Silver;
            }
            else if (tbPassword.Text.Length < 16)
            {
                MessageBox.Show("Длина ключа должна быть 16 символов!");
                tbPassword.BorderBrush = Brushes.Red;
                btEncode.IsEnabled = false;
                btDecode.IsEnabled = false;
                Encrypt.IsEnabled = false;
                Decrypt.IsEnabled = false;
                Generate_keys.IsEnabled = false;
            }
            else
            {
                uint[] key = AlgoTea.CreateKey(Encoding.Default.GetBytes(tbPassword.Text));
                K0.Text = Convert.ToString(key[0], 2);
                K1.Text = Convert.ToString(key[1], 2);
                K2.Text = Convert.ToString(key[2], 2);
                K3.Text = Convert.ToString(key[3], 2);
                btEncode.IsEnabled = true;
                btDecode.IsEnabled = true;
                Encrypt.IsEnabled = true;
                Decrypt.IsEnabled = true;
                Generate_keys.IsEnabled = true;
                tbPassword.BorderBrush = Brushes.Silver;
            }
        }

    }
}

