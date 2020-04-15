set FORGE_CLIENT_ID=<your_client_id>
set FORGE_CLIENT_SECRET=<your_client_secret>
set FORGE_BIM_ACCOUNT_ID=<your_account_id>
cd ..
Autodesk.BimProjectSetup.exe -p ".\sample\BIM360_Projects_Template.csv" -b "https://developer.api.autodesk.com" -t ";" -z "," -e "UTF-8" -d "yyyy-MM-dd" -r false -h "admin.account@yourcompny.com"
pause