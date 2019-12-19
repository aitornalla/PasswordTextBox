using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PasswordTextBox
{
    public partial class Form1 : Form
    {
        private StringBuilder m_passwrod = new StringBuilder(string.Empty);     // Character password
        private int m_insertPos = 0;                                            // Position where the cursor is in the textbox
        private char m_maskChar = '*';                                          // Password special character mask

        private Task m_asteriskTask;                                            // Asterisktation task variable
        private CancellationTokenSource m_asteriskTaskCTS;                      // Cancellation token source for asterisktation task
        private int m_timeToPutAsterisk = 450;                                  // Time to wait before asterisking last typed character

        private bool debug = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_asteriskTaskCTS?.Cancel();
            m_asteriskTaskCTS?.Dispose();
            //m_asteriskTask?.Dispose();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            // User deletes all content
            if (textBox1.Text == string.Empty)
            {
                // Check if previuos asynchronous asterisktation task is completed
                if (m_asteriskTask != null && !m_asteriskTask.IsCompleted)
                {
                    // If not completed, cancel previous task
                    m_asteriskTaskCTS.Cancel();
                }
                // Clear password member
                m_passwrod.Clear();
                // (Debug) Set passwrod memeber into show passwrod textbox
                textBox2.Text = m_passwrod.ToString();

                return;
            }

            // User writes a character
            if (textBox1.Text.Length > m_passwrod.Length)
            {
                // Save cursor position
                m_insertPos = textBox1.SelectionStart;
                // Insert new character into password member
                m_passwrod.Insert(m_insertPos - 1, textBox1.Text.Substring(m_insertPos - 1, 1));
                // (Debug) Set passwrod memeber into show password textbox
                textBox2.Text = m_passwrod.ToString();
                // If show password is checked just put password in textbox, else asterisk password
                if (checkBox1.Checked)
                {
                    textBox1.Text = m_passwrod.ToString();
                }
                else
                {
                    // Check if previuos asynchronous asterisktation task is completed
                    if (m_asteriskTask != null && !m_asteriskTask.IsCompleted)
                    {
                        // If not completed, cancel previous task
                        m_asteriskTaskCTS.Cancel();

                        StringBuilder l_ast = new StringBuilder();
                        // Asterisk all characters except the new one
                        for (int i = 0; i < textBox1.Text.Length; i++)
                        {
                            if (i == (m_insertPos - 1))
                            {
                                l_ast.Append(m_passwrod.ToString().Substring(m_insertPos - 1, 1));
                            }
                            else
                            {
                                l_ast.Append(m_maskChar);
                            }
                        }

                        textBox1.Text = l_ast.ToString();
                    }
                    // Create new cancellation token to pass to the asterisktation task
                    m_asteriskTaskCTS = new CancellationTokenSource();
                    m_asteriskTask = WaitToPutAsteriskAsync(m_asteriskTaskCTS.Token);
                }
                // Set cursor position as it was before the event (if password textbox is assigned a new value, SelectionStart property gets reset)
                textBox1.SelectionStart = m_insertPos;

                return;
            }

            // User deletes a character
            if (textBox1.Text.Length < m_passwrod.Length)
            {

                // Save cursor position
                m_insertPos = textBox1.SelectionStart;
                // Remove character from passwrod member
                m_passwrod.Remove(textBox1.SelectionStart, 1);
                // (Debug) Set passwrod memeber into show passwrod textbox
                textBox2.Text = m_passwrod.ToString();
                // If show password is checked just put password in textbox, else asterisk password
                if (checkBox1.Checked)
                {
                    textBox1.Text = m_passwrod.ToString();
                }
                else
                {
                    // Check if previuos asynchronous asterisktation task is completed
                    if (m_asteriskTask != null && !m_asteriskTask.IsCompleted)
                    {
                        // If not completed, cancel previous task
                        m_asteriskTaskCTS.Cancel();
                    }


                    StringBuilder l_ast = new StringBuilder();
                    // Asterisk password textbox
                    for (int i = 0; i < textBox1.Text.Length; i++)
                        l_ast.Append(m_maskChar);

                    textBox1.Text = l_ast.ToString();
                }
                // Set cursor position as it was before the event (if password textbox is assigned a new value, SelectionStart property gets reset)
                textBox1.SelectionStart = m_insertPos;

                return;
            }
        }

        /// <summary>
        ///     Asterisk last character typed by the user after waiting a fixed time. If the users types or deletes a character the task is cancelled
        /// </summary>
        /// <param name="canTok">Cancellation token. See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        private async Task WaitToPutAsteriskAsync(CancellationToken canTok)
        {
            if (await WaitToPutAsterisk(canTok))
                return;

            StringBuilder l_ast = new StringBuilder();
            // Asterisk all characters
            for (int i = 0; i < textBox1.Text.Length; i++)
                l_ast.Append(m_maskChar);

            // Get current cursor position as the user may have moved its position after the waiting period
            int l_pos = textBox1.SelectionStart;

            textBox1.Text = l_ast.ToString();

            textBox1.SelectionStart = l_pos;
        }

        /// <summary>
        ///     Task that waits a fixed time before finishing and enabling asterisking
        /// </summary>
        /// <param name="canTok">Cancellation token. See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        private Task<bool> WaitToPutAsterisk(CancellationToken canTok)
        {
            return Task.Run(() =>
            {
                System.Diagnostics.Stopwatch l_stopwatch = new System.Diagnostics.Stopwatch();
                l_stopwatch.Start();
                // Instead of using Thread.Sleep() we use a while and the method can end either because elapsed time is greater than time to put asterisk or task cancellation
                while (l_stopwatch.Elapsed.TotalMilliseconds < m_timeToPutAsterisk && !canTok.IsCancellationRequested)
                {

                }
                // Return task cancellation requested
                return canTok.IsCancellationRequested;
            },
                canTok
            );
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            // Check if previuos asynchronous asterisktation task is completed
            if (m_asteriskTask != null && !m_asteriskTask.IsCompleted)
            {
                // If not completed, cancel previous task
                m_asteriskTaskCTS.Cancel();
            }

            // Get current cursor position as the user may have moved its position after the waiting period
            int l_pos = textBox1.SelectionStart;

            if (checkBox1.Checked)
            {
                textBox1.Text = m_passwrod.ToString();
            }
            else
            {
                StringBuilder l_ast = new StringBuilder();
                // Asterisk all characters
                for (int i = 0; i < textBox1.Text.Length; i++)
                    l_ast.Append(m_maskChar);

                textBox1.Text = l_ast.ToString();
            }

            textBox1.SelectionStart = l_pos;
        }
    }
}
