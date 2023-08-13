using BatchHardLink.AppCode;
using System.ComponentModel;
using System.Text;

namespace BatchHardLink
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox4.Clear();
            StringBuilder error = new StringBuilder();
            if(!FileHelper.PathCheck(textBox1.Text, textBox2.Text, label3.Text, ref error))
            {
                MessageBox.Show(error.ToString());
                return;
            }

            FileHelper.DoAfterCreate = (fullName) =>
            {
                DisplayMessage(fullName);
            };

            var except = new HashSet<string>() { ".torrent" };
            if (!string.IsNullOrWhiteSpace(textBox3.Text))
            {
                except = textBox3.Text.ToLower().Split('.')
                    .Where(i => !string.IsNullOrEmpty(i)).Select(i => "." + i).ToHashSet();
            }

            long? limitSize = null;
            if (!string.IsNullOrWhiteSpace(textBox5.Text))
            {
                try
                {
                    limitSize = Convert.ToInt64(textBox5.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("最小限制仅限输入数字");
                }
            }

            var task = Task.Run(()=>
            {
                try
                {
                    FileHelper.BatchCreateHardLink(textBox1.Text, textBox2.Text, 
                        except, limitSize);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("系统异常：" + ex.ToString());
                }
                finally
                {
                    label3.Text = "";
                }
            });

            label3.Text = "后台创建中，请稍后...";
        }

        private void DisplayMessage(string content)
        {
            if (textBox4.TextLength >= 20480)
            {
                textBox4.Clear();
            }
            textBox4.AppendText($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} -> {content}" + Environment.NewLine);
            textBox4.ScrollToCaret();
        }

    }
}