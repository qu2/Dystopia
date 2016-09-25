/* Dystopia (@3dzn) */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MetroFramework.Forms;
using Windows.UI.Notifications;

namespace yamb2
{
    public partial class Form1 : MetroForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        // 起動時の処理
        private void Form1_Load(object sender, EventArgs e)
        {
            // ミナコイを開く
            browser.Url = new Uri(Properties.Settings.Default.topUrlChange);

            // ブックマークの復元 (XML -> Object)
            XmlSerializer serializer = new XmlSerializer(typeof(List<Bookmark>));
            List<Bookmark> bookmarkList = null;

            using (StreamReader reader = new StreamReader("bookmark.xml", Encoding.UTF8))
            {
                bookmarkList = (List<Bookmark>)serializer.Deserialize(reader);
            }

            for (int i = 0; i < bookmarkList.Count; i++)
            {
                lstBkm.Items.Add(bookmarkList[i]);
            }

            // XMLから設定画面へ
            // トップページを変更
            this.boxChangeTop.Text = Properties.Settings.Default.topUrlChange;
            // ダイス変更
            this.boxChangeDice.Text = Properties.Settings.Default.diceChange;
            // 連投文章の変更
            this.boxChangeTrollText.Text = Properties.Settings.Default.TrollText;
        }

        // 終了時の処理
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // ブックマークの保存 (Object -> XML)
            List<Bookmark> bookmarkList = new List<Bookmark>();

            for (int i = 0; i < lstBkm.Items.Count; i++)
            {
                bookmarkList.Add((Bookmark)lstBkm.Items[i]);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(List<Bookmark>));
            using (StreamWriter writer = new StreamWriter("bookmark.xml", false, Encoding.UTF8)) {
                serializer.Serialize(writer, bookmarkList);
            }
        }

        // target="_blank"を無効
        private void Browser_NewWindow(object sender, CancelEventArgs e)
        {
            WebBrowser wb = (WebBrowser)sender;
            string txt = wb.StatusText;
            if (txt != "")
            {
                browser.Navigate(txt);
            }
            e.Cancel = true;
        }

        // ブラウザの読み込みが終わったら実行するやつ
        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // ウェブページのタイトルを表示
            string wTitle;
            wTitle = browser.DocumentTitle;
            webTitle.Text = wTitle;
        }

        // SS
        private void btnTop_Click(object sender, EventArgs e) // btnTopになっているのはボタン名を変えるのがめんどくさかったからです
        {
            string random = Guid.NewGuid().ToString("N").Substring(0, 4);

            Rectangle rc = Screen.PrimaryScreen.Bounds;
            Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + random + "SS.bmp";
            bmp.Save(filePath, ImageFormat.Bmp);
        }

        // << BACK
        private void btnBack_Click(object sender, EventArgs e)
        {
            browser.GoBack();
        }

        // NEXT >>
        private void btnNext_Click(object sender, EventArgs e)
        {
            browser.GoForward();
        }

        // ブラウザ読み込み後の処理
        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            // URLの表示
            urlBox.Text = browser.Url.ToString();

            // 行くページがないときにボタンを無効化
            btnBack.Enabled = browser.CanGoBack;
            btnNext.Enabled = browser.CanGoForward;

            // プログレスバー
            loadBar.Value = 100;
        }

        // ブラウザ読み込み中の処理
        private void browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            // プログレスバー
            loadBar.Value = 25;
            loadBar.Value = 50;
        }

        // EnterでURL先に移動
        private void urlBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter))
            {
                // URLの形式が正しくないときに警告を出す
                try
                {
                    browser.Url = new Uri(urlBox.Text);
                }
                catch
                {
                    MessageBox.Show("URLの形式が正しくありません。", "Dystopia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ブックマーク
        private void lstBkm_DoubleClick(object sender, EventArgs e)
        {
            Bookmark data = (Bookmark) lstBkm.Items[lstBkm.SelectedIndex];
            browser.Url = new Uri(data.Url);
        }

        // ブックマークをクラスに入れて管理
        public class Bookmark
        {
            public String Title = "";
            public String Url = "";

            public override String ToString()
            {
                return Title;
            }
        }

        // ブックマーク追加
        private void addFavorite_Click(object sender, EventArgs e)
        {
            // データの設定
            Bookmark data = new Bookmark();
            data.Title = browser.DocumentTitle;
            data.Url = browser.Url.ToString();

            // 重複した場合に登録させない
            if (Exists(data) == true)
            {
                // ブクマがダブったとき
                MessageBox.Show("既に同じブックマークが登録済みです。", "Dystopia", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // リストに追加
            lstBkm.Items.Add(data);
        }

        // ブックマークリストチェック
        private bool Exists(Bookmark data1)
        {
            for (int i = 0; i < lstBkm.Items.Count; i++)
            {
                Bookmark data2 = (Bookmark)lstBkm.Items[i];

                if (data1.Url.Equals(data2.Url))
                {
                    // ブクマがダブったとき
                    return true;
                }
            }
            // ダブらなかったとき
            return false;
        }

        // ブックマーク削除
        private void delFavorite_Click(object sender, EventArgs e)
        {
            // 選択されている部分の取得
            try
            {
                Bookmark data = (Bookmark)lstBkm.Items[lstBkm.SelectedIndex];

                // 削除
                lstBkm.Items.Remove(data);
            }
            catch
            {
                MessageBox.Show("削除するブックマークが指定されていないか、ブックマークが存在しません。", "Dystopia", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 初期表示ウェブページの変更(ボタン)
        private void btnChangeTop_CheckedChanged(object sender, EventArgs e)
        {
            if (btnChangeTop.Checked)
            {
                this.boxChangeTop.Enabled = true;
            }
            else
            {
                this.boxChangeTop.Enabled = false;
            }
        }

        // ダイス変更(ボタン)
        private void btnChangeDice_CheckedChanged(object sender, EventArgs e)
        {
            if (btnChangeDice.Checked)
            {
                this.boxChangeDice.Enabled = true;
            }
            else
            {
                this.boxChangeDice.Enabled = false;
            }
        }

        // 連投文章変更(ボタン)
        private void btnChangeTrollText_CheckedChanged(object sender, EventArgs e)
        {
            if (btnChangeTrollText.Checked)
            {
                this.boxChangeTrollText.Enabled = true;
            }
            else
            {
                this.boxChangeTrollText.Enabled = false;
            }
        }

        // 設定セーブボタン(General)
        private void btnSave_Click(object sender, EventArgs e)
        {
            // トップページ変更
            Properties.Settings.Default.topUrlChange = this.boxChangeTop.Text;
            // ダイス変更
            Properties.Settings.Default.diceChange = this.boxChangeDice.Text;
            // 連投文章変更
            Properties.Settings.Default.TrollText = this.boxChangeTrollText.Text;

            // 保存
            Properties.Settings.Default.Save();

            // 完了通知
            // Windows 8以降
            try
            {
                var tmpl = ToastTemplateType.ToastImageAndText02;
                var xml = ToastNotificationManager.GetTemplateContent(tmpl);

                var images = xml.GetElementsByTagName("image");
                var src = images[0].Attributes.GetNamedItem("src");
                src.InnerText = "http://";

                var texts = xml.GetElementsByTagName("text");
                texts[0].AppendChild(xml.CreateTextNode("Dystopia Settings"));
                texts[1].AppendChild(xml.CreateTextNode("設定を保存しました。"));

                var toast = new ToastNotification(xml);

                ToastNotificationManager.CreateToastNotifier("Dystopia").Show(toast);
            }
            // Windows 7
            catch
            {
                MessageBox.Show("設定を保存しました。", "Dystopia Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ダイス
        private void btnDice_Click(object sender, EventArgs e)
        {
            try
            {
                HtmlElementCollection all = browser.Document.All;
                HtmlElementCollection forms = all.GetElementsByName("chat");
                forms[0].InnerText = (Properties.Settings.Default.diceChange);
                SendKeys.Send("{ENTER}");
            }
            catch
            {
                MessageBox.Show("ここでサイコロは使えません。", "Dystopia", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /* 定型文 */
        // こんにちは
        private void fpHello_Click(object sender, EventArgs e)
        {
            try
            {
                HtmlElementCollection all = browser.Document.All;
                HtmlElementCollection forms = all.GetElementsByName("chat");
                forms[0].InnerText = ("こんにちは");
                SendKeys.Send("{ENTER}");
            }
            catch
            {
                MessageBox.Show("ここで定型文は使用できません。", "Dystopia", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // こんばんは
        private void fpGE_Click(object sender, EventArgs e)
        {
            try
            {
                HtmlElementCollection all = browser.Document.All;
                HtmlElementCollection forms = all.GetElementsByName("chat");
                forms[0].InnerText = ("こんばんは");
                SendKeys.Send("{ENTER}");
            }
            catch
            {
                MessageBox.Show("ここで定型文は使用できません。", "Dystopia", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 連投
        private void btnTroll_Click(object sender, EventArgs e)
        {
            btnStopTroll.Visible = true;
            bTimer.Interval = 1000;
            bTimer.Enabled = true;
        }
        private void bTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                string random = Guid.NewGuid().ToString("N").Substring(0, 4);
                string trollText = Properties.Settings.Default.TrollText;

                HtmlElementCollection all = browser.Document.All;
                HtmlElementCollection forms = all.GetElementsByName("chat");
                forms[0].InnerText += (trollText + "(" + random + (")"));

                SendKeys.Send("{ENTER}");
            }
            catch
            {
                bTimer.Enabled = false;
                MessageBox.Show("ここで連投は使用できません。", "Dystopia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStopTroll.Visible = false;
            }
        }

        // 連投ストップ
        private void btnStopTroll_Click(object sender, EventArgs e)
        {
            bTimer.Enabled = false;
            btnStopTroll.Visible = false;
        }
    }
}

/* MetroFramework (https://raw.githubusercontent.com/thielj/MetroFramework/master/LICENSE.md) */
