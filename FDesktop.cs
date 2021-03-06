using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Snapsy.wia;
using Snapsy.twain;
using WIA;
using System.Drawing.Imaging;

namespace Snapsy
{
    public partial class FDesktop : Form
    {
        private SortedList<int, CScannedImage> images;
        public static CScanSettings LastProfile { get; set; }

        public FDesktop()
        {
            InitializeComponent();
            images = new SortedList<int, CScannedImage>();
        }

        private void thumbnailList1_ItemActivate(object sender, EventArgs e)
        {
            FViewer viewer = new FViewer(images[(int)thumbnailList1.SelectedItems[0].Tag].BaseImage);
            viewer.Show();
        }

        private void updateView()
        {
            thumbnailList1.UpdateView(images);
        }

        private void demoScan()
        {
            /*string path = Application.StartupPath + "\\demo";
            string[] files = Directory.GetFiles(path);
            Array.Sort(files);
            foreach (string file in files)
            {
                int next = images.Count > 0 ? images.Keys[images.Count - 1] + 1 : 0;
                Image img = Image.FromFile(file);
                images.Add(next, img);
            }
            updateView();*/
        }

        private void scanWIA(CScanSettings Profile)
        {
            CWIAAPI api;
            try
            {
                api = new CWIAAPI(Profile);
            }
            catch (Exceptions.EScannerNotFound)
            {
                MessageBox.Show("Device not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CScannedImage img = api.GetImage();

            if (img != null)
            {
                int next = images.Count > 0 ? images.Keys[images.Count - 1] + 1 : 0;
                images.Add(next, img);
            }
            thumbnailList1.UpdateImages(images);
            Application.DoEvents();

            if (Profile.Source != CScanSettings.ScanSource.GLASS)
            {
                while (img != null)
                {
                    img = api.GetImage();
                    if (img != null)
                    {
                        int next = images.Count > 0 ? images.Keys[images.Count - 1] + 1 : 0;
                        images.Add(next, img);
                    }
                    thumbnailList1.UpdateImages(images);
                    Application.DoEvents();
                }
            }
        }

        private void scanTWAIN(string DeviceName)
        {
            CTWAINAPI twa;
            try
            {
                twa = new CTWAINAPI(DeviceName, this);
            }
            catch (Exceptions.EScannerNotFound)
            {
                MessageBox.Show("Device not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<CScannedImage> scanned = twa.Scan();
            foreach (CScannedImage bmp in scanned)
            {
                int next = images.Count > 0 ? images.Keys[images.Count - 1] + 1 : 0;
                images.Add(next, bmp);
            }
            thumbnailList1.UpdateImages(images);
        }

        private void deleteItems()
        {
            if (thumbnailList1.SelectedItems.Count > 0)
            {
                if (MessageBox.Show(string.Format("Do you really want to delete {0} items?", thumbnailList1.SelectedItems.Count), "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    foreach (ListViewItem delitem in thumbnailList1.SelectedItems)
                    {
                        images.Remove((int)delitem.Tag);
                    }
                    thumbnailList1.UpdateImages(images);
                }
            }
        }

        private int getImageBefore(int id)
        {
            int ret = 0;
            foreach (int cid in images.Keys)
            {
                if (cid == id)
                    return ret;
                ret = cid;
            }
            return 0;
        }

        private int getImageAfter(int id)
        {
            bool found = false;
            foreach (int cid in images.Keys)
            {
                if (found)
                    return cid;
                if (cid == id)
                    found = true;
            }
            return 0;
        }

        private void moveUp()
        {
            if (thumbnailList1.SelectedItems.Count > 0)
            {
                if (thumbnailList1.SelectedItems[0].Index > 0)
                {
                    foreach (ListViewItem it in thumbnailList1.SelectedItems)
                    {
                        int before = getImageBefore((int)it.Tag);
                        CScannedImage temp = images[before];
                        images[before] = images[(int)it.Tag];
                        images[(int)it.Tag] = temp;
                        thumbnailList1.Items[thumbnailList1.Items.IndexOf(it) - 1].Selected = true;
                        it.Selected = false;
                    }
                    updateView();
                }
            }
        }

        private void moveDown()
        {
            if (thumbnailList1.SelectedItems.Count > 0)
            {
                if (thumbnailList1.SelectedItems[thumbnailList1.SelectedItems.Count - 1].Index < images.Count - 1)
                {

                    for (int i = thumbnailList1.SelectedItems.Count - 1; i >= 0; i--)
                    {
                        ListViewItem it = thumbnailList1.SelectedItems[i];
                        int after = getImageAfter((int)it.Tag);
                        CScannedImage temp = images[after];
                        images[after] = images[(int)it.Tag];
                        images[(int)it.Tag] = temp;
                        thumbnailList1.Items[thumbnailList1.Items.IndexOf(it) + 1].Selected = true;
                        it.Selected = false;
                    }
                    updateView();
                }
            }
        }

        private void rotateLeft()
        {
            if (thumbnailList1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem it in thumbnailList1.SelectedItems)
                {
                    images[(int)it.Tag].RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
                updateView();
            }
        }

        private void rotateRight()
        {
            if (thumbnailList1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem it in thumbnailList1.SelectedItems)
                {
                    images[(int)it.Tag].RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
                updateView();
            }
        }

        private void thumbnailList1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    quickScan_Click(null, null);
                    break;
                case Keys.Delete:
                    deleteItems();
                    break;
                case Keys.Left:
                    if (e.Control)
                        moveUp();
                    break;
                case Keys.Right:
                    if (e.Control)
                        moveDown();
                    break;
            }
        }

        private void exportPDFProcess(string filename, FPDFSave dialog)
        {
            PdfDocument document = new PdfDocument();
            document.Layout = PdfSharp.Pdf.IO.PdfWriterLayout.Compact;

            int nameStart = filename.LastIndexOf('\\') + 1;
            document.Info.Title = filename.Substring(nameStart, filename.Length - nameStart - ".pdf".Length);
            document.Info.Subject = document.Info.Title;
            document.Info.Author = GetLoginName();
            document.Info.Creator = "Snapsy PDF Scanner";
            int i = 1;
            foreach (CScannedImage img in images.Values)
            {
                ThreadStart setstatus = delegate { dialog.SetStatus(i, images.Count); };
                dialog.Invoke(setstatus);

                double realWidth = img.BaseImage.Width / img.BaseImage.HorizontalResolution * 72;
                double realHeight = img.BaseImage.Height / img.BaseImage.VerticalResolution * 72;
                PdfPage newPage = document.AddPage();
                newPage.Width = (int)realWidth;
                newPage.Height = (int)realHeight;
                XGraphics gfx = XGraphics.FromPdfPage(newPage);
                gfx.DrawImage(ConvertImageToFormat(img.BaseImage, ImageFormat.Jpeg, 90), 0, 0, (int)realWidth, (int)realHeight);
                i++;
            }
            document.Save(filename);
            dialog.Invoke(new ThreadStart(dialog.Close));
            System.Diagnostics.Process.Start(filename);
        }

        private static String GetLoginName()
        {
            return new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent()).Identity.Name;
        }

        /// <summary>
        /// Converts a given image to jpeg format with the given quality level
        /// </summary>
        /// <returns>Compressed image in jpeg format</returns>
        private static Image ConvertImageToFormat(Image img, ImageFormat format, long quality)
        {
            MemoryStream memStream = new MemoryStream();
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            img.Save(memStream, GetEncoder(format), encoderParameters);
            return Image.FromStream(memStream);
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

        private void exportPDF(string filename)
        {
            FPDFSave pdfdialog = new FPDFSave();
            ThreadStart starter = delegate { exportPDFProcess(filename, pdfdialog); };
            new Thread(starter).Start();
            pdfdialog.ShowDialog(this);
        }

        private void quickScan_Click(object sender, EventArgs e)
        {
            if (LastProfile == null)
            {
                tsScan_Click(sender, e);
            }
            else
            {
                if (LastProfile.DeviceDriver == CScanSettings.Driver.WIA)
                    scanWIA(LastProfile);
                else
                    scanTWAIN(LastProfile.DeviceID);
            }
        }

        private void tsScan_Click(object sender, EventArgs e)
        {
            //demoScan();
            //return;

            FChooseProfile prof = new FChooseProfile();
            prof.ShowDialog();

            if (prof.Profile == null)
                return;
            else
                LastProfile = prof.Profile;

            if (prof.Profile.DeviceDriver == CScanSettings.Driver.WIA)
                scanWIA(prof.Profile);
            else
                scanTWAIN(prof.Profile.DeviceID);
        }

        private void tsSavePDF_Click(object sender, EventArgs e)
        {
            if (images.Count > 0)
            {

                SaveFileDialog sd = new SaveFileDialog();
                sd.OverwritePrompt = true;
                sd.AddExtension = true;
                sd.Filter = "PDF document (*.pdf)|*.pdf";

                if (sd.ShowDialog() == DialogResult.OK)
                {
                    exportPDF(sd.FileName);
                }
            }
        }

        private void tsSaveImage_Click(object sender, EventArgs e)
        {
            if (images.Count > 0)
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.OverwritePrompt = true;
                sd.AddExtension = true;
                sd.Filter = "Supported image files (*.jpg;*.jpeg;*.gif;*.png;*.bmp;*.tif)|*.jpg;*.jpeg;*.gif;*.png;*.bmp;*.tif";
                sd.DefaultExt = "jpg";

                if (sd.ShowDialog() != DialogResult.OK)
                    return;

                if (Path.GetExtension(sd.FileName) == ".tif")
                {
                    Image[] imgs = new Image[images.Values.Count];
                    for (int i = 0; i < images.Values.Count; i++)
                        imgs[i] = images.Values[i].BaseImage;
                    CTiffHelper.SaveMultipage(imgs, sd.FileName);
                }
                else
                {
                    if (images.Count == 1)
                        images.Values[0].BaseImage.Save(sd.FileName);
                    else
                        for (int i = 0; i < images.Values.Count; i++)
                        {
                            string filename = Path.GetDirectoryName(sd.FileName) + "\\" + Path.GetFileNameWithoutExtension(sd.FileName) + " " + i.ToString().PadLeft(images.Values.Count / 10, '0') + Path.GetExtension(sd.FileName);
                            saveImageToFilename(images.Values[i].BaseImage, filename);
                        }
                }
            }
        }
        /// <summary>
        /// Saves images in bmp, emf, exif, gif, jpg or png format depending on the file extension in filename
        /// </summary>
        /// <param name="img">Image to save</param>
        /// <param name="filename">Full filename to save to, including path + extension</param>
        private static void saveImageToFilename(Image img, String filename)
        {
            Image convertedImage = null;
            switch (Path.GetExtension(filename))
            {
                case ".bmp":
                    convertedImage = ConvertImageToFormat(img, ImageFormat.Bmp, 100);
                    break;
                case ".gif":
                    convertedImage = ConvertImageToFormat(img, ImageFormat.Gif, 100);
                    break;
                case ".jpg":
                case ".jpeg":
                    convertedImage = ConvertImageToFormat(img, ImageFormat.Jpeg, 100);
                    break;
                case ".png":
                    convertedImage = ConvertImageToFormat(img, ImageFormat.Png, 100);
                    break;
                default:
                    MessageBox.Show("Don't know how to save in " + Path.GetExtension(filename) + " format; please enter a known format (.bmp, .gif, .jpg, .jpeg, or .png)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            convertedImage.Save(filename);
        }

        private void tsPDFEmail_Click(object sender, EventArgs e)
        {
            if (images.Count > 0)
            {
                string path = Application.StartupPath + "\\Scan.pdf";
                exportPDF(path);
                MAPI.CMAPI.SendMail(path, "");
                File.Delete(path);
            }
        }

        private void tsMoveUp_Click(object sender, EventArgs e)
        {
            moveUp();
        }

        private void tsMoveDown_Click(object sender, EventArgs e)
        {
            moveDown();
        }

        private void tsRotateLeft_Click(object sender, EventArgs e)
        {
            rotateLeft();
        }

        private void tsRotateRight_Click(object sender, EventArgs e)
        {
            rotateRight();
        }

        private void tsProfiles_Click(object sender, EventArgs e)
        {
            FManageProfiles pmanager = new FManageProfiles();
            pmanager.ShowDialog();
        }

        private void tsAbout_Click(object sender, EventArgs e)
        {
            new FAbout().ShowDialog();
        }

        private void tsExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void deleteAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to delete ALL scanned items?", "Delete All Items?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                images.Clear();
                thumbnailList1.UpdateImages(images);
            }
        }
    }
}