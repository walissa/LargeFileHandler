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
    public partial class ResetMessageBody : IBaseComponent, IComponent, IComponentUI, IPersistPropertyBag
    {
        [RequiredRuntime]
        [DisplayName("Temp Folder")]
        [Description("The Folder where the large files will moved to before processing further in the send side.")]
        public string TempFolder { get; set; }

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
            //Temp folder path, it must be
            string newPath = Path.Combine(Path.GetDirectoryName(receivedFile), TempFolder);
            string newFilePath = Path.Combine(newPath, Path.GetFileName(receivedFile));
            //Create the temp directory if it does not exist.
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            //We need to close the file handle, so we can move it to the temp folder.
            originalStream.Close();

            //Moving the received file to the temp folder, so it will not be picked up again nor deleted after the pipeline is fully executed.
            File.Move(receivedFile, newFilePath);
            pInMsg.Context.Promote(ctxPropReceivedFileName, newFilePath);
            var ms = new MemoryStream();
            pContext.ResourceTracker.AddResource(ms);
            pInMsg.BodyPart.Data = ms;
            return pInMsg;
        }
    }
}