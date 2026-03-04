namespace tulo.CoreLib.PDFs
{
    public static class HtmlPdfRenderer
    {
        public static string CreateHtmlViewerFromPdf(MemoryStream pdfStream)
        {
            if (pdfStream == null || pdfStream.Length == 0)
            {
                return "<html><body><h1>PDF not found or empty</h1></body></html>";
            }

            pdfStream.Position = 0;
            byte[] pdfBytes = pdfStream.ToArray();
            string base64Pdf = Convert.ToBase64String(pdfBytes);

            return $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>PDF Viewer</title>
                        <style>
                            body {{
                                margin: 0;
                                padding: 0;
                                overflow: hidden;
                            }}
                            iframe {{
                                position: absolute;
                                top: 0;
                                left: 0;
                                width: 100%;
                                height: 100%;
                                border: none;
                            }}
                        </style>
                    </head>
                    <body>
                        <iframe src='data:application/pdf;base64,{base64Pdf}'></iframe>
                    </body>
                    </html>";
        }
    }
}