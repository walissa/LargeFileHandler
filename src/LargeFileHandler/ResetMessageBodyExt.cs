using System;
using BizTalkComponents.Utils;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using IComponent = Microsoft.BizTalk.Component.Interop.IComponent;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace BizTalkComponents.PipelineComponents.LargeFileHandler
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Decoder)]
    [System.Runtime.InteropServices.Guid("a040f5b6-6fbf-499d-9c92-596d1601c691")]
    public partial class ResetMessageBodyExt : IBaseComponent, IComponent, IComponentUI, IPersistPropertyBag
    {
        [RequiredRuntime]
        [DisplayName("Temp Folder")]
        [Description("The temp folder full path where the large files will written to before processing further in the send side.")]
        public string TempFolder { get; set; }


        [RequiredRuntime]
        [DisplayName("Buffer Size(KB)")]
        [Description("The buffer size in KB to use while wrting the message body to the temp folder, if it is set to 0, the default size will be used 4KB=4096 bytes")]
        public int BufferSize { get; set; }

        public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            if (Disabled)
            {
                return pInMsg;
            }

            string errorMessage;
            if (!Validate(out errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }
            var originalStream = pInMsg.BodyPart.GetOriginalDataStream();
            //context property ReceivedFileName
            var ctxPropReceivedFileName = new ContextProperty("http://schemas.microsoft.com/BizTalk/2003/file-properties#ReceivedFileName");
            //The original received file path.
            string receivedFile = (string)pInMsg.Context.Read(ctxPropReceivedFileName);
            if (string.IsNullOrEmpty(receivedFile))
            {
                receivedFile = Guid.NewGuid().ToString();
            }
            //Temp folder path, it must be
            string newFilePath = Path.Combine(TempFolder, Path.GetFileName(receivedFile));
            //Create the temp directory if it does not exist.
            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }

            var fs = new FileStream(newFilePath, FileMode.Create);
            if (BufferSize == 0) BufferSize = 4;
            var data = new byte[1024 * BufferSize];
            int i = 1,j=0;

            while (i > 0)
            {
                i = originalStream.Read(data, 0, data.Length);
                fs.Write(data, 0, i);
                Task.Delay(0).GetAwaiter().GetResult();
                j++;
                if (j % 50 == 0)
                    fs.Flush();
            }
            fs.Flush();
            fs.Close();
            fs.Dispose();
            originalStream.Close();
            pInMsg.Context.Promote(ctxPropReceivedFileName, newFilePath);
            var ms = new MemoryStream();
            pContext.ResourceTracker.AddResource(ms);
            pInMsg.BodyPart.Data = ms;
            return pInMsg;
        }
    }
}