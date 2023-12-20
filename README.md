[![Build status](https://dev.azure.com/waal/BizTalk%20Components/_apis/build/status/BizTalk%20Components/LargeFileHandler)](https://dev.azure.com/waal/BizTalk%20Components/_build/latest?definitionId=21)

# Large File Handler

Large File handler is a BizTalk based solution consists of custom pipeline components serving large file transfer in BizTalk without sending the received file to MsgBox database.
These components are:
- Reset Message Body
- Reset Message Body Extended
- Set Message Body From FileStream
## Components
### Reset Message Body
This component is to be used in the Decode stage, it will replace the message body with an empty one, and it will move the file to a temp folder under the same received file location.
It updates the context property ReceivedFileName to refer to the new location of the received file (under the temp folder).
The purpose of the using this component under <strong>Decode</strong> is to make sure that the received file will not be written to MsgBox.

| Parameter | Description | Type | Validation |
|-|-|-|-|
|Temp Folder| An accepted folder name where the received files will moved to |String|Required, example TempFolder = Temp|
|Disabled |Set to True to disable the component, default value = False|Bool|Required|


### Reset Message Body Extended
This component is to be used in the Decode stage, it will replace the message body with an empty one, and it will write body to a file in the specified temp folder.
It updates the context property ReceivedFileName to refer to the file location of the received body.
The purpose of the using this component under <strong>Decode</strong> is to make sure that the received file will not be written to MsgBox.

| Parameter | Description | Type | Validation |
|-|-|-|-|
|Temp Folder| The folder path where the received messages will written to |String|Required, example TempFolder = C:\BTData\INT0123\In\Temp|
|Buffer Size| The size of the buffer will be used for writing the message to the temp folder, the larger size the faster, BT server memory to be considered while setting this value, if the size=0, the default buffer will be used (4KB=4096 bytes) |Integer|Required, example Buffer Size = 10240 (10 MB)|
|Disabled |Set to True to disable the component, default value = False|Bool|Required|

### Set Body From FileStream
This component is to be used in the Encode stage, it reads the ReceivedFileName context property and extracts the large file location, it replaces the message body with the FileStream for the large file, then the adapter used in the send port will manage to send out the large file stream.

| Parameter | Description | Type | Validation |
|-|-|-|-|
|Disabled |Set to True to disable the component, default value = False|Bool|Required|

## Remarks ##
- Tracking message bodies before the port processing in the receive port should not be checked.
- Tracking message bodies after the port processing in the send port should not be checked.
- Files moved to the temp folder must be deleted.


