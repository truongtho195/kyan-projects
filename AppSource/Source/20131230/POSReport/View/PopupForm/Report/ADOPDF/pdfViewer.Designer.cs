namespace CPC.POSReport.View.PopupForm.Report.ADOPDF
{
    partial class pdfViewer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(pdfViewer));
            this.pdfReportFile = new AxAcroPDFLib.AxAcroPDF();
            ((System.ComponentModel.ISupportInitialize)(this.pdfReportFile)).BeginInit();
            this.SuspendLayout();
            // 
            // pdfReportFile
            // 
            this.pdfReportFile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pdfReportFile.Enabled = true;
            this.pdfReportFile.Location = new System.Drawing.Point(3, 0);
            this.pdfReportFile.Name = "pdfReportFile";
            this.pdfReportFile.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("pdfReportFile.OcxState")));
            this.pdfReportFile.Size = new System.Drawing.Size(297, 272);
            this.pdfReportFile.TabIndex = 0;
            // 
            // pdfViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pdfReportFile);
            this.Name = "pdfViewer";
            this.Size = new System.Drawing.Size(300, 272);
            ((System.ComponentModel.ISupportInitialize)(this.pdfReportFile)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxAcroPDFLib.AxAcroPDF pdfReportFile;


    }
}
