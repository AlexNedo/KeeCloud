## Process for establishing dropbox sync
### Getting the dropbox refreshToken
 - In the Menu click Tools -> URL Credential Wizard
 - a dropbox page is opened in the browser with a code
 - Copy the code into the field in the wizard
 - keecloud will exchange the code for an refreshToken which can be used as a password for dropbox api requests. There is also a button to save the refreshToken as an keepass entry
### Seting up a trigger for syncing to dropbox on database save
 - in the menu Tools -> Triggers
 - add a trigger
 - Events: Saved database file
 - Add three actions:
   - Change trigger on/off state  with Off
   - Syncronize active databasewith a file/Url
     - URL is:  dropbox://subfolder/PWDataBase.kdbx
     - username is empty
     - password is the refreshToken from the previous step
     - the kdbx file needs to be present in dropbox for syncing
   - Change trigger on/off state  with On

## notes for building the plgx
 - remove all files from bin/obj folders, because Keepass will do its own compilation
 - set an appropriate version in the AssemblyInfo.cs
 - set the appkey and appsecret in the Api.cs in the Dropbox folder
 - copy the dependent dlls listed in the csproj next to the csproj
 - adjust the hintpaths from "..\packages\long\lib-name\path\lib.dll" to just ".\lib.dll"
 - call  keepass2 --plgx-create  /path/to/the/folder-with-the-csproj
 - the plgx is a package with the source. Keepass will compile it on start

## general notes

It is not necessary to being able to compile the project manually. The source along with the csproj and the dependent dlls can be put in a plgx file. 
During the start Keepass will compile the dll and use it.

The initial error why I had to fix this plugin was either:
 - dropbox disabled TLS version < 1.2 support which is not used by default in net 4.5
 - the dropbox accesstoken just expired 
The error message was not meaningful, which is why I dont know the exact reason

The accessToken are short lived. Therefore refreshTokens are now used.


