using System;
using System.Xml;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Printing;

namespace DynamicFormsByXML
{
    class Program
    {
        public string filename = @"C:\DynamicFormsByXML\FormDefinition.xml";
        public Dictionary<string, string> listOfLocks = new Dictionary<string, string>();

        // Print form screen-shot
        private PrintDocument printDocument = new PrintDocument();
        Bitmap memoryImage;
        Form memoryForm = new Form();
        int mainCount = 0;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Program myApp = new Program();
            Application.Run(myApp.Start());
        }

        public Form Start()
        {
            // Initialise the form
            Form form = new Form() { Text = "Dynamic Forms", Icon = DynamicFormsByXML.Properties.Resources.FormIcon };
            form.SuspendLayout();

            // Load the XML
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filename);

            // Load attributes for form
            if (xmlDoc.DocumentElement.Name == "Form")
            {
                foreach (XmlAttribute attribute in xmlDoc.DocumentElement.Attributes)
                {
                    if (string.Equals(attribute.Name.ToLower(), "reference"))
                        form.Text = attribute.Value;
                }
            }

            // Load controls
            XmlElement root = xmlDoc.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("Form.Controls");
            foreach (XmlNode node in nodes[0].ChildNodes)
                RenderControls(form, node);

            // Show the form
            form.ResumeLayout();
            form.Width = 300;
            form.Height = 210;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            SetEnables(form);
            form.Show();

            return form;
        }

        private void CaptureScreen(Form form)
        {
            // We are going to change some properties of the form make it look like a document in the print
            FormBorderStyle border = form.FormBorderStyle;
            form.FormBorderStyle = FormBorderStyle.None;
            Color color = form.BackColor;
            form.BackColor = Color.WhiteSmoke;

            Graphics myGraphics = form.CreateGraphics();
            Size s = form.Size;
            memoryImage = new Bitmap(s.Width, s.Height, myGraphics);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(form.Location.X, form.Location.Y, 0, 0, s);

            // Restore old properties
            form.FormBorderStyle = border;
            form.BackColor = color;
        }

        private void printDocument_PrintPage(Object sender, PrintPageEventArgs e)
        {
            // More info: http://stackoverflow.com/questions/15276984/printing-all-controls-in-a-winform-c-sharp-using-printdocument
            // TODO: Potencially this will face when we the form area to print is bigger than the page... this needs some ameds...
            Font printFont = new Font("Arial", 9);
            int dgX = memoryForm.Left;
            int dgY = memoryForm.Top += 22;
            double linesPerPage = 0;
            float yPos = 0;
            int count = 0;

            float leftMargin = e.MarginBounds.Left;
            float topMargin = e.MarginBounds.Top;
            float bottomMargin = e.MarginBounds.Bottom;
            StringFormat str = new StringFormat();

            linesPerPage = e.MarginBounds.Height / printFont.GetHeight(e.Graphics);
            Control ctrl;

            while ((count < linesPerPage) && (memoryForm.Controls.Count != mainCount))
            {
                //int pageOffset = currentPageNumber * heightOfPrintableArea;
                ctrl = memoryForm.Controls[mainCount];
                yPos = topMargin + (count * printFont.GetHeight(e.Graphics));
                mainCount++;
                count++;
                if (ctrl is Label)
                {
                    //e.Graphics.DrawString(ctrl.Text, printFont, Brushes.Black, ctrl.Left + 5, ctrl.Top + 40 - pageOffset);
                    e.Graphics.DrawString(ctrl.Text, printFont, Brushes.Black, ctrl.Left + 5, ctrl.Top + 40);
                }
                else if (ctrl is TextBox)
                {
                    //e.Graphics.DrawString(ctrl.Text, printFont, Brushes.Black, ctrl.Left + 5, ctrl.Top + 40 - pageOffset);
                    e.Graphics.DrawString(ctrl.Text, printFont, Brushes.Black, ctrl.Left + 5, ctrl.Top + 40);
                    //e.Graphics.DrawRectangle(Pens.Black, ctrl.Left, ctrl.Top + 40, ctrl.Width, ctrl.Height);
                }
                else if (ctrl is CheckBox)
                {
                    //e.Graphics.DrawString(ctrl.Text, printFont, Brushes.Black, ctrl.Left + 5, ctrl.Top + 40 - pageOffset);
                    e.Graphics.DrawString(ctrl.Text + ": " + (((CheckBox)ctrl).Checked == true ? "True" : "False"), printFont, Brushes.Black, ctrl.Left + 5, ctrl.Top + 40);
                }
            }
            if (count > linesPerPage)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
            }
        }

        private void printDocument_PrintScreenShot(Object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(memoryImage, 0, 0);
        }

        void Print(Form form)
        {
            // Enable print
            printDocument.PrintPage += new PrintPageEventHandler(printDocument_PrintPage);

            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;

            //Show Print Dialog
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                //Print the form
                memoryForm = form;
                printDocument.Print();
            }
        }

        void PrintScreenShot(Form form)
        {
            // Links event
            printDocument.PrintPage += new PrintPageEventHandler(printDocument_PrintScreenShot);

            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;

            //Show Print Dialog
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                //Print the form
                CaptureScreen(form);
                printDocument.Print();
            }
        }

        void RenderControls(Form form, XmlNode node)
        {
            switch (node.Name.ToLower())
            {
                //=-=-=-=-=-=
                case "label":
                //=-=-=-=-=-=
                    // Declares control
                    Label label = new Label();

                    // Attributes
                    foreach (System.Xml.XmlAttribute attribute in node.Attributes)
                    {
                        if (string.Equals(attribute.Name.ToLower(), "name"))
                            label.Name = attribute.Value;
                        if (string.Equals(attribute.Name.ToLower(), "text"))
                            label.Text = attribute.Value;
                    }

                    // Properties
                    SetUpBasicProperties(node, label);

                    // Add the control to the form
                    label.AutoSize = true;
                    form.Controls.Add(label);
                    break;


                //=-=-=-=-=-=-=
                case "textbox":
                //=-=-=-=-=-=-=
                    // Declares control
                    TextBox textBox = new TextBox();

                    // Attributes
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if (string.Equals(attribute.Name.ToLower(), "name"))
                            textBox.Name = attribute.Value;
                        if (string.Equals(attribute.Name.ToLower(), "content"))
                            textBox.Text = attribute.Value;
                    }

                    // Properties
                    SetUpBasicProperties(node, textBox);

                    // Add the control to the form
                    form.Controls.Add(textBox);
                    break;


                //=-=-=-=-=-=-=-=
                case "checkbox":
                //=-=-=-=-=-=-=-=
                    // Declares control
                    CheckBox checkBox = new CheckBox();

                    // Attributes
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if (string.Equals(attribute.Name.ToLower(), "name"))
                            checkBox.Name = attribute.Value;
                        if (string.Equals(attribute.Name.ToLower(), "text"))
                            checkBox.Text = attribute.Value;
                        if (string.Equals(attribute.Name.ToLower(), "content"))
                            checkBox.Checked = string.Equals(attribute.Value.ToLower(), "true") ? true : false;
                    }

                    // Properties
                    SetUpBasicProperties(node, checkBox);

                    // Add the control to the form
                    checkBox.UseVisualStyleBackColor = true;
                    checkBox.AutoSize = true;

                    // Re-process attributes to find locks (at this point we need the rest of the attributes first)
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if (string.Equals(attribute.Name.ToLower(), "locks"))
                        {
                            checkBox.Click += (sender, e) => SetEnables(form);
                            listOfLocks.Add(checkBox.Name, attribute.Value);
                        }
                    }

                    form.Controls.Add(checkBox);
                    break;


                //=-=-=-=-=-=-=
                case "button":
                //=-=-=-=-=-=-=
                    // Declares control
                    Button button = new Button();

                    // Attributes
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if (string.Equals(attribute.Name.ToLower(), "name"))
                            button.Name = attribute.Value;
                        if (string.Equals(attribute.Name.ToLower(), "text"))
                            button.Text = attribute.Value;
                        if (string.Equals(attribute.Name.ToLower(), "onclick") && string.Equals(attribute.Value.ToLower(), "save"))
                            button.Click += (sender, e) => Save(form);
                        if (string.Equals(attribute.Name.ToLower(), "onclick") && string.Equals(attribute.Value.ToLower(), "printscreenshot"))
                            button.Click += (sender, e) => PrintScreenShot(form);
                        if (string.Equals(attribute.Name.ToLower(), "onclick") && string.Equals(attribute.Value.ToLower(), "print"))
                            button.Click += (sender, e) => Print(form);
                    }

                    // Properties
                    SetUpBasicProperties(node, button);

                    // Add the control to the form
                    form.Controls.Add(button);
                    break;
            }
        }

        void Save(Form form)
        {
            // Load the XML
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filename);

            // Loop through controls to see if the content has changed
            foreach (Control control in form.Controls)
            {
                // Save the changes for TextBoxes
                if (control is TextBox || control is CheckBox)
                {
                    XmlElement root = xmlDoc.DocumentElement;
                    XmlNodeList nodes = root.SelectNodes("Form.Controls");
                    foreach (XmlNode node in nodes[0].ChildNodes)
                    {
                        // This solution will only allow us to save one value as the internal enumeration list of attributes can not be changed in execution,
                        // if we need to change several attributes, we will have to store them in a list and then save the changes after the loop ends, never
                        // inside the loop.
                        XmlDocument doc;
                        XmlAttribute attr = null;

                        foreach (XmlAttribute attribute in node.Attributes)
                        {
                            if (string.Equals(attribute.Name.ToLower(), "name") && string.Equals(attribute.Value, control.Name))
                            {
                                //Get the document object
                                doc = node.OwnerDocument;

                                //Create a new attribute
                                attr = doc.CreateAttribute("Content");
                                if (control is TextBox)
                                    attr.Value = control.Text;
                                else if (control is CheckBox)
                                    attr.Value = ((CheckBox)control).Checked.ToString();
                            }
                        }

                        //Add the attribute to the node     
                        if (attr != null)
                            node.Attributes.SetNamedItem(attr);
                    }
                }
            }

            // Save changes in the XML file
            xmlDoc.Save(filename);
        }

        void SetEnables(Form form)
        {
            foreach (KeyValuePair<string, string> item in listOfLocks)
            {
                Control controlBlocker = null;
                Control controlToBeLocked = null;

                foreach (Control control in form.Controls)
                {
                    // Control Blocker
                    if (string.Equals(control.Name.ToLower(), item.Key.ToLower()) && ((CheckBox)control).Checked)
                        controlBlocker = control;
                    // Control To Be Locked
                    if (string.Equals(control.Name.ToLower(), item.Value.ToLower()))
                        controlToBeLocked = control;
                }
                
                if (controlBlocker != null && controlToBeLocked != null)
                    controlToBeLocked.Enabled = false;
                else
                    controlToBeLocked.Enabled = true;
            }
        }

        void SetUpBasicProperties(XmlNode node, Control control)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (string.Equals(child.Name, "Size"))
                {
                    foreach (XmlAttribute attribute in child.Attributes)
                    {
                        if (string.Equals(attribute.Name.ToLower(), "width"))
                            control.Width = Int32.Parse(attribute.Value);
                        if (string.Equals(attribute.Name.ToLower(), "height"))
                            control.Height = Int32.Parse(attribute.Value);
                    }
                }

                if (string.Equals(child.Name, "Location"))
                {
                    int x = 0, y = 0;
                    foreach (XmlAttribute attribute in child.Attributes)
                    {
                        if (string.Equals(attribute.Name.ToLower(), "x"))
                            x = Int32.Parse(attribute.Value);
                        if (string.Equals(attribute.Name.ToLower(), "y"))
                            y = Int32.Parse(attribute.Value);
                    }
                    control.Location = new Point(x, y);
                }
            }
        }
    }
}
