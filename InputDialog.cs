using System.Drawing;
using System.Windows.Forms;

namespace GenericForms
{
    public class InputDialog
    {
        public static DialogResult ShowInputDialog(ref string input, string formTitle, string formPrompt)
        {
            Size size = new Size(400, 0);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = formTitle;
            inputBox.AutoSize = true;

            Label label = new Label();
            label.MaximumSize = new Size(size.Width, 0);
            label.AutoSize = true;
            label.Location = new Point(10, 10);
            label.Text = formPrompt;
            inputBox.Controls.Add(label);

            TextBox textBox = new TextBox();
            textBox.Size = new Size(size.Width - 20, 0);
            textBox.Location = new Point(10, label.Size.Height + 20);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new Point(size.Width - 80 - 90, label.Size.Height + textBox.Size.Height + 30);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new Point(size.Width - 85, label.Size.Height + textBox.Size.Height + 30);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }
    }
}
