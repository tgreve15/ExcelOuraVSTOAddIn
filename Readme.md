# ExcelOuraVSTOAddIn
An Excel VSTO Add-In that will retrieve your Oura ring data from the Oura Cloud into your Excel document.
Written in C# using Visual Studio Community 2019 edition. 
To use this you will have to go into your Oura Dashboard (https://cloud.ouraring.com/dashboard) and 
select "Oura Developer" (https://cloud.ouraring.com/personal-access-tokens) from there, under "Personal 
Access Tokens", "Create New Personal Access Token" to use for this. Copy that token, and you will have to 
enter it in the app.Config in the appropriate place. Once this is in place, you should be able to run and 
execute the extension.  
If you would prefer to use OAuth2 Authentication, feel free to implement this, but please ensure by default
this add-in is left configured for Personal Access Token use.

This add-In will allow you to select which fields of information you want to download and in what order, for 
what time frame (by default a week, but you can go for years), and if you want headers to be included or not. 

NOTE 2: Just in case you need it, it appears the "live" configuration file for the add-in is located under
C:\Users\<profile name>\AppData\Local\Microsoft_Corporation
in a folder with the name starting "ExcelOuraVSTOAddIn"

The latest installable version is available under the "_Releases" folder and here - https://github.com/tgreve15/ExcelOuraVSTOAddIn/blob/f2a6c4eb41db4dec15efaf00f71829dfe64479bb/_Releases/ExcelOuraVSTOAddIn_1_0_0_8.zip
