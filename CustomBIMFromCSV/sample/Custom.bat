set FORGE_CLIENT_ID=<your_client_id_without_quotation_marks>
set FORGE_CLIENT_SECRET=<your_client_secret_without_quotation_marks>
set FORGE_BIM_ACCOUNT_ID=<your_account_id_without_quotation_marks>
cd ..
CustomBIMFromCSV.exe -p ".\sample\BIM360_Custom_Template.csv" -h "accountAdmin@company.com" -t "," -z "," -e "UTF-8" -d "yyyy-MM-dd"
pause