[![Build status](https://waal.visualstudio.com/BizTalk%20Components/_apis/build/status/BizTalk%20Components/LargeFileHandler)](https://waal.visualstudio.com/BizTalk%20Components/_build/latest?definitionId=21)

# Large File Handler

Large File handler is a BizTalk based solution consists of two custom pipeline components serving large file transfer in BizTalk without sending the received file to MsgBox database.
These components are:
- Reset Message Body
- Set Message Body From FileStream
## Components
### Reset Message Body
This component is to be used in the Decode stage, it will replace the message body with a simple xml ```<root/>``` and it will move the file to a temp folder under the same received file location.
It updates the context property ReceivedFileName to refer to the new location of the received file (under the temp folder).
The purpose of the using this component under <strong>Decode</strong> is to make sure that the received file will not be written to MsgBox.

| Parameter | Description | Type | Validation |
|-|-|-|-|
|Temp Folder| An accepted folder name where the received files will moved to |String|Required, example TempFolder = Temp|
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

For further information about using these components, please visit [Simplify-IT.info](https://simplify-it.info/)