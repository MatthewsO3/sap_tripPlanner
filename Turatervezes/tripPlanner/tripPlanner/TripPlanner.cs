using SAPbobsCOM;
using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using tripPlanner.Services.Form;


namespace tripPlanner
{
    class TripPlanner
    {
        public static SAPbouiCOM.Application SBO_Application;
        private int[] docEntries = new int[0];
        public TripPlanner()
        {
            // Az SAP Business One-hoz való kapcsolódás
            #region "Connection"
            SAPbouiCOM.SboGuiApi SboGuiApi = null;
            string sConnectionString = null;
            SboGuiApi = new SAPbouiCOM.SboGuiApi();

            try
            {
                if (Environment.GetCommandLineArgs().Length > 1)
                    sConnectionString = Environment.GetCommandLineArgs().GetValue(1).ToString();
                else
                    sConnectionString = "";

                if (sConnectionString.Length == 0)
                    sConnectionString = "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056";

                SboGuiApi.Connect(sConnectionString);
                SBO_Application = SboGuiApi.GetApplication(-1);
                globalD.oSBOApp = SBO_Application;
            }
            catch (Exception ex)
            {
                var errorString = ex.Message.ToString();
                MessageBox.Show("Nem lehet kapcsolódni a SAP B1 GUI-hoz!\n" + ex.Message.ToString());
                System.Environment.Exit(0);
            }

            try
            {
                globalD.oCompany = new SAPbobsCOM.Company();
                string sCookie = null;
                sCookie = globalD.oCompany.GetContextCookie();
                string sConnectionContext = null;
                sConnectionContext = SBO_Application.Company.GetConnectionContext(sCookie);
                globalD.oCompany.SetSboLoginContext(sConnectionContext);
                if (globalD.oCompany.Connect() != 0)
                {
                    MessageBox.Show("Nem lehet kapcsolódni a SAP B1 DI API-hoz!\n" + globalD.oCompany.GetLastErrorDescription());
                    System.Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nem lehet kapcsolódni a SAP B1 DI API-hoz!\n" + ex.Message.ToString());
                System.Environment.Exit(0);
            }
            #endregion "Connection"

            foreach (SAPbouiCOM.Form oForm in SBO_Application.Forms)
            {
                if (oForm.TypeEx == "0") oForm.Close();
            }

            SBO_Application.StatusBar.SetText(System.Windows.Forms.Application.ProductName + " kapcsolódott a SAP B1 rendszerhez....", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
            createMenu();
            SBO_Application.MenuEvent += new _IApplicationEvents_MenuEventEventHandler(SBO_Application_MenuEvent);
            SBO_Application.ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBO_Application_ItemEvent);
        }

        private void createMenu()
        {
            try
            {
                SAPbouiCOM.Menus oMenus = SBO_Application.Menus;
                string parentMenuId = "2048"; // - Beszerzés
                string subMenuId = "TripPlanner";

                if (!oMenus.Exists(subMenuId))
                {
                    SAPbouiCOM.MenuCreationParams oMenuParams = (SAPbouiCOM.MenuCreationParams)SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams);
                    oMenuParams.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                    oMenuParams.UniqueID = subMenuId;
                    oMenuParams.String = "Túratervezés";
                    oMenuParams.Position = 1;
                    oMenuParams.Enabled = true;

                    oMenus.Item(parentMenuId).SubMenus.AddEx(oMenuParams);
                }
            }
            catch (Exception ex)
            {
                SBO_Application.StatusBar.SetText("Hiba a menü létrehozása közben: " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }

        private void SBO_Application_MenuEvent(ref SAPbouiCOM.MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            if (pVal.BeforeAction == false && pVal.MenuUID == "TripPlanner")
            {
                try
                {
                    SBO_Application.StatusBar.SetText("Loading TripPlanner form...", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    try
                    {
                        SAPbouiCOM.Form existingForm = SBO_Application.Forms.Item("tripPlanner_form");
                        existingForm.Close();
                        SBO_Application.StatusBar.SetText("Closed existing form with UID: tripPlanner_form", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                    }
                    catch
                    {
                    }

                   using (var formService = new FormService("turatervezo_form.xml"))
                    {
                        string formXML = formService.ExportToString(new Dictionary<string, string>
                        {
                            { "@[FormType]", "turatervezo_form" },
                            { "@[FormUID]", "tripPlanner_form" }
                        });

                        SBO_Application.StatusBar.SetText("XML Length: " + formXML.Length, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);

                        SBO_Application.LoadBatchActions(ref formXML);
                        SAPbouiCOM.Form form = SBO_Application.Forms.Item("tripPlanner_form");
                        form.Title = "Túratervezés";
                        form.Left = 100;
                        form.Top = 100;
                        form.Width = 1050;
                        form.Height = 560;
                        form.Visible = true;


                        SBO_Application.StatusBar.SetText("Form loaded successfully", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    }
                }
                catch (Exception ex)
                {
                    SBO_Application.StatusBar.SetText("Error loading form: " + ex.Message + " | Stack: " + ex.StackTrace, BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Error);
                }
            }
        }

        private void SBO_Application_ItemEvent(string FormUID, ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            SAPbouiCOM.Form oForm = null;
            
            try
            {
                if (pVal.EventType == BoEventTypes.et_FORM_UNLOAD || pVal.EventType == BoEventTypes.et_FORM_DEACTIVATE || pVal.EventType == BoEventTypes.et_FORM_CLOSE)
                {
                    return;
                }

                oForm = SBO_Application.Forms.GetForm(pVal.FormTypeEx, pVal.FormTypeCount);

                switch (pVal.EventType)
                {
                    case BoEventTypes.et_ITEM_PRESSED:
                        if (pVal.FormTypeEx == "60004" && pVal.ItemUID == "Item_3" && !pVal.BeforeAction)
                        {
                            try
                            {
                                LoadMatrix(oForm);
                            }
                            catch (Exception ex)
                            {
                                SBO_Application.StatusBar.SetText("Error executing query: " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            }
                        }
                        break;
                    case BoEventTypes.et_MATRIX_LINK_PRESSED:
                            if (pVal.FormTypeEx == "60004" && !pVal.BeforeAction)
                            {
                                try
                                {
                                    if (pVal.ColUID == "order_id")
                                    {
                                        var oItem = oForm.Items.Item("Item_7");
                                        var oMatrix = oItem.Specific as SAPbouiCOM.Matrix;
                                        var ordEditText = oMatrix.GetCellSpecific("order_id", pVal.Row) as EditText;
                                        var ord = ordEditText.Value;
                                        SBO_Application.OpenForm(BoFormObjectEnum.fo_Order, "", docEntries[pVal.Row - 1].ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SBO_Application.StatusBar.SetText("Error linked button" + ex.Message + " | Stack: " + ex.StackTrace, BoMessageTime.bmt_Long, BoStatusBarMessageType.smt_Error);

                                }
                                
                            }
                        break;

                }
               
            }
            catch (Exception ex)
            {
                SBO_Application.StatusBar.SetText($"ItemEvent Error: {pVal.EventType} - {ex.Message}", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            if (pVal.FormUID == "tripPlanner_form" && pVal.EventType == BoEventTypes.et_ITEM_PRESSED && pVal.ItemUID == "Item_8" && !pVal.BeforeAction)
            {
                saveMatrix(oForm, docEntries);
              
            }
        }
        private void saveMatrix(SAPbouiCOM.Form oForm, int[] docEntries)
        {
            try
            {
                SAPbouiCOM.DataTable oDT;

                SAPbobsCOM.GeneralService oGeneralService = null;
                SAPbobsCOM.GeneralData oGeneralData = null;
                SAPbobsCOM.GeneralDataParams oGeneralParams = null;
                SAPbobsCOM.CompanyService sCmp = null;
               

                Recordset oRs = (Recordset)globalD.oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                 
                if (oForm == null)
                {
                    return;
                }

                oDT = oForm.DataSources.DataTables.Item("DT_0");
                if (oDT == null)
                {
                    return;
                }
                sCmp = globalD.oCompany.GetCompanyService();
                oGeneralService = sCmp.GetGeneralService("NTT_TOUR");
                SAPbouiCOM.Item gridItem = oForm.Items.Item("Item_7");
                SAPbouiCOM.Matrix matrix = (SAPbouiCOM.Matrix)gridItem.Specific;
                matrix.FlushToDataSource(); 

                for (int i = 0; i < oDT.Rows.Count; i++)
                {

                    string docEntry = oDT.Columns.Item("db_ordid").Cells.Item(i).Value;
                    string query = $"SELECT \"DocEntry\" FROM \"@NTT_TOUR\" WHERE \"U_ord_id\" = '{docEntry}'";
                    oRs.DoQuery(query);

                    if (oRs.RecordCount > 0)
                    {
                        int docEntryValue = Convert.ToInt32(oRs.Fields.Item("DocEntry").Value);
                        oGeneralParams = (SAPbobsCOM.GeneralDataParams)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams);
                        oGeneralParams.SetProperty("DocEntry", docEntryValue);
                        oGeneralData = oGeneralService.GetByParams(oGeneralParams);
                    }
                    else
                    {
                        oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);
                        oGeneralData.SetProperty("U_ord_id", docEntry);
                    }

                    oGeneralData.SetProperty("U_date", oDT.Columns.Item("db_date").Cells.Item(i).Value);
                    oGeneralData.SetProperty("U_lic_num", oDT.Columns.Item("db_lic").Cells.Item(i).Value);
                    oGeneralData.SetProperty("U_driver", oDT.Columns.Item("db_driver").Cells.Item(i).Value);
                    oGeneralData.SetProperty("U_circle", oDT.Columns.Item("db_circle").Cells.Item(i).Value);
                    oGeneralData.SetProperty("U_ord_name", oDT.Columns.Item("db_ordnm").Cells.Item(i).Value);
                    oGeneralData.SetProperty("U_del_addr", oDT.Columns.Item("db_addr").Cells.Item(i).Value);
                    oGeneralData.SetProperty("U_ord_kg", oDT.Columns.Item("db_ordkg").Cells.Item(i).Value);
                    oGeneralData.SetProperty("U_tour_kg", oDT.Columns.Item("db_tourkg").Cells.Item(i).Value);
                    oGeneralData.SetProperty("U_ord_desc", oDT.Columns.Item("db_descor").Cells.Item(i).Value);
                    oGeneralData.SetProperty("U_desc_addr", oDT.Columns.Item("db_descad").Cells.Item(i).Value);
                    oGeneralData.SetProperty("U_days", oDT.Columns.Item("db_days").Cells.Item(i).Value);
                   // oGeneralData.SetProperty("U_car_kg", oDT.Columns.Item("db_carkg").Cells.Item(i).Value);

                    // Rekord mentése
                    if (oRs.RecordCount == 0)
                    {
                        oGeneralService.Add(oGeneralData);
                        SBO_Application.StatusBar.SetText("Save completed : ADD", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);

                    }
                    else
                    {
                        oGeneralService.Update(oGeneralData);
                        SBO_Application.StatusBar.SetText("Save completed : Update", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);


                    }
                    SAPbobsCOM.Documents oOrder = (SAPbobsCOM.Documents)globalD.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
                     if (oOrder.GetByKey(docEntries[i]))
                     {
                         // Frissítsd a megfelelő User Defined Field-eket


                         string rendszamName = oDT.Columns.Item("db_lic").Cells.Item(i).Value;
                         string soforName = oDT.Columns.Item("db_driver").Cells.Item(i).Value;
                         string szallkorName = oDT.Columns.Item("db_circle").Cells.Item(i).Value;

                         string rendszamCode = GetCodeFromName("@NTT_RENDSZAM", rendszamName);
                         string soforCode = GetCodeFromName("@NTT_SOFOR", soforName);
                         string szallkorCode = GetCodeFromName("@NTT_SZALLKOR", szallkorName);
                         oOrder.UserFields.Fields.Item("U_NTT_RENDSZAM").Value = rendszamCode;
                         oOrder.UserFields.Fields.Item("U_NTT_SOFOR").Value = soforCode;
                         oOrder.UserFields.Fields.Item("U_NTT_SZALLKOR").Value = szallkorCode;


                         // Mentsd a változásokat
                         if (oOrder.Update() != 0)
                         {
                             string errMsg;
                             int errCode;
                             globalD.oCompany.GetLastError(out errCode, out errMsg);
                             SBO_Application.StatusBar.SetText("ORDR update failed: " + errMsg, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                         }
                         else
                         {
                             SBO_Application.StatusBar.SetText("ORDR updated successfully", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                         }
                     }

                }

                // Űrlap frissítése
                oForm.DataSources.DataTables.Item("DT_0").ExecuteQuery("SELECT * FROM [@NTT_TOUR] ORDER BY DocEntry");
            }
            catch (Exception ex)
            {
                SBO_Application.StatusBar.SetText("Error during save: " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
           


        }
        
        private void checkIfOverLimit(DataTable oDT, Matrix matrix)
        {
            int red = (255 & 0xFF) | ((80 & 0xFF) << 8) | ((80 & 0xFF) << 16);

            for (int i = 0; i < oDT.Rows.Count; i++)
            {
                decimal cap, tour;
                string capStr = oDT.Columns.Item("db_carkg").Cells.Item(i).Value?.ToString();
                string tourStr = oDT.Columns.Item("db_tourkg").Cells.Item(i).Value?.ToString();

                if (!decimal.TryParse(capStr, NumberStyles.Float, CultureInfo.InvariantCulture, out cap))
                {

                    SBO_Application.StatusBar.SetText(
                        $"Érvénytelen érték a(z) {i + 1}. sorban a car_kg oszlopban",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }

                if (!decimal.TryParse(tourStr, NumberStyles.Float, CultureInfo.InvariantCulture, out tour))
                {
                    SBO_Application.StatusBar.SetText(
                        $"Érvénytelen érték a(z) {i + 1}. sorban a tour_kg oszlopban",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }

                if (tour >= cap)
                {
                    matrix.CommonSetting.SetRowBackColor(i + 1, red);
                    SBO_Application.StatusBar.SetText(
                        $"carcap: {cap}, tour: {tour}, rows loaded",
                        BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }

            }
        }
        private void LoadMatrix(SAPbouiCOM.Form oForm)
        {
            string rendszam = oForm.DataSources.UserDataSources.Item("UD_0").Value;
            string szallKor = oForm.DataSources.UserDataSources.Item("UD_1").Value;
            string szallDatum = oForm.DataSources.UserDataSources.Item("UD_2").Value;
            string rendszamParam = string.IsNullOrEmpty(rendszam) ? "NULL" : $"N'{rendszam.Replace("'", "''")}'";
            string szallKorParam = string.IsNullOrEmpty(szallKor) ? "NULL" : $"N'{szallKor.Replace("'", "''")}'";
            string szallDatumParam = string.IsNullOrEmpty(szallDatum) ? "NULL" : $"N'{szallDatum.Replace("'", "''")}'";

            SAPbouiCOM.DataTable oDT;
            oDT = oForm.DataSources.DataTables.Item("DT_0");
            oDT.Rows.Clear();


            string query = $"EXEC NTT_TURATERVEZES @rendszam = {rendszamParam}, @szallKor = {szallKorParam}, @szallDatum = {szallDatumParam}";
            var oRs = (SAPbobsCOM.Recordset)globalD.oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            SAPbouiCOM.DataTable dt = oForm.DataSources.DataTables.Item("DT_0");
            oRs.DoQuery(query);

            if (oRs.RecordCount == 0)
            {
                SBO_Application.StatusBar.SetText("Nincs találat!", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            else
            {
                docEntries = new int[oRs.RecordCount];
                oDT.Rows.Clear();
                for (int i = 0; i < oRs.RecordCount; i++)
                {
                    oDT.Rows.Add();
                    docEntries[i] = oRs.Fields.Item("docEntry").Value;
                    oDT.Columns.Item("db_ordid").Cells.Item(i).Value = oRs.Fields.Item("order_id").Value;

                    oDT.Columns.Item("db_date").Cells.Item(i).Value = oRs.Fields.Item("del_date").Value;

                    oDT.Columns.Item("db_lic").Cells.Item(i).Value = oRs.Fields.Item("car_id").Value;
                    oDT.Columns.Item("db_driver").Cells.Item(i).Value = oRs.Fields.Item("driver").Value;
                    oDT.Columns.Item("db_circle").Cells.Item(i).Value = oRs.Fields.Item("del_circle").Value;
                    oDT.Columns.Item("db_ordnm").Cells.Item(i).Value = oRs.Fields.Item("order_name").Value;
                    oDT.Columns.Item("db_addr").Cells.Item(i).Value = oRs.Fields.Item("del_addr").Value;
                    oDT.Columns.Item("db_ordkg").Cells.Item(i).Value = oRs.Fields.Item("order_kg").Value;
                    oDT.Columns.Item("db_tourkg").Cells.Item(i).Value = oRs.Fields.Item("tour_kg").Value;
                    oDT.Columns.Item("db_descor").Cells.Item(i).Value = oRs.Fields.Item("desc_ord").Value;
                    oDT.Columns.Item("db_descad").Cells.Item(i).Value = oRs.Fields.Item("desc_addr").Value;
                    oDT.Columns.Item("db_days").Cells.Item(i).Value = oRs.Fields.Item("days").Value;
                    oDT.Columns.Item("db_carkg").Cells.Item(i).Value = oRs.Fields.Item("car_kg").Value;
                    oRs.MoveNext();


                }
                SAPbouiCOM.Item gridItem = oForm.Items.Item("Item_7");
                SAPbouiCOM.Matrix matrix = (SAPbouiCOM.Matrix)gridItem.Specific;
                matrix.AutoResizeColumns();

                matrix.LoadFromDataSource();
                checkIfOverLimit(oDT, matrix);


                SBO_Application.StatusBar.SetText($"Query executed, {oDT.Rows.Count} rows loaded", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
            }
        }
        private string GetCodeFromName(string tableName, string name)
        {
            Recordset oRs = (Recordset)globalD.oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            string query = $"SELECT \"Code\" FROM \"{tableName}\" WHERE \"Name\" = '{name.Replace("'", "''")}'";
            oRs.DoQuery(query);

            if (oRs.RecordCount > 0)
            {
                return oRs.Fields.Item("Code").Value.ToString();
            }
            return "";
        }
    }
}
