using SAPbobsCOM;
using SAPbouiCOM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using tripPlanner.Services.Form;


namespace tripPlanner
{
    class TripPlanner
    {
        public static SAPbouiCOM.Application SBO_Application;

        

        public TripPlanner()
        {
            // Az SAP Business One-hoz való kapcsolódás
            #region "Connection"
            SAPbouiCOM.SboGuiApi SboGuiApi = null;
            string sConnectionString = null;
            SboGuiApi = new SAPbouiCOM.SboGuiApi();

            try
            {
                //Debug módban az else ágba fog belefutni
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

            // Ha debugolás közben beragadt egy MessageBox, akkor indítsd újra az addont, és ez bezárja.
            // Bezárjuk a beragadt üzenetablakokat.
            foreach (SAPbouiCOM.Form oForm in SBO_Application.Forms)
            {
                if (oForm.TypeEx == "0") oForm.Close();
            }

            SBO_Application.StatusBar.SetText(System.Windows.Forms.Application.ProductName + " kapcsolódott a SAP B1 rendszerhez....", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
            // Itt kell megadni, hogy milyen eseménycsoportokat szeretnénk lekezelni. Például: MenuEvent, ItemEvent, RightClickEvent, FormDataEvent 
            createMenu();
            SBO_Application.MenuEvent += new _IApplicationEvents_MenuEventEventHandler(SBO_Application_MenuEvent);
            SBO_Application.ItemEvent += new SAPbouiCOM._IApplicationEvents_ItemEventEventHandler(SBO_Application_ItemEvent);
            // SBO_Application.FormDataEvent += new SAPbouiCOM._IApplicationEvents_FormDataEventEventHandler(ref SBO_Application_FormDataEvent);
            
            
        }
      
        private void createMenu()
        {
            try
            {
             
                SAPbouiCOM.Menus oMenus = SBO_Application.Menus;
                string parentMenuId = "2304"; // - Beszerzés
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

        private void SBO_Application_MenuEvent(ref MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            if (pVal.BeforeAction == false && pVal.MenuUID == "TripPlanner")
            {
                try
                {

                    string formXML = (new FormService("turatervezo_form.xml")).ExportToString(new Dictionary<string, string>
                    {
                        { "@[FormType]", "turetervezo_form" },
                        { "@[FormUID]", "asdad" }
                    });
                    
                    // Load the form using XML
                    SBO_Application.LoadBatchActions(ref formXML);

                    // Get the form after it's created from XML
                    SAPbouiCOM.Form form = SBO_Application.Forms.Item("tripPlanner_form");

                    form.Title = "Túratervezés";
                    form.Width = 1050;
                    form.Height = 560;
                    form.Visible = true;
                }
                catch(Exception ex)
                {
                    SBO_Application.StatusBar.SetText("From h " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);

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
                // Beállítjuk, hogy melyik SAP-s ablakban történt az esemény
                // Érdemes switch case segítségével kezelni a különböző típusú eseményeket
                switch (pVal.EventType)
                {
                    // Az ablak betöltésének eseménye. Itt kell hozzáadnunk a formhoz az új gombokat, mezőket, füleket
                    case BoEventTypes.et_FORM_LOAD:
                        if (!pVal.BeforeAction && pVal.ActionSuccess && pVal.FormTypeEx == "141")
                        {
                            oForm = SBO_Application.Forms.GetForm(pVal.FormTypeEx, pVal.FormTypeCount);

                            //Meghívjuk azt a metódust, ami létrehozza az új gombot
                           
                        }

                        break;
                    // Ezt az eseményt kell használni, ha a felhasználó fülre, gombra, check box-ra kattint. Az esemény akkor hívódik meg, amikor a felhasználó ezen elemek egyikén kattintás után (mouse up)
                    // Van egy et_Click esemény is, a
                    case BoEventTypes.et_ITEM_PRESSED:
                        {
                            oForm = SBO_Application.Forms.GetForm(pVal.FormTypeEx, pVal.FormTypeCount);

                            if (!pVal.BeforeAction && pVal.ActionSuccess && pVal.ItemUID == "BTN" && oForm.Items.Item(pVal.ItemUID).Enabled)
                            {
                                // Meghívjuk azt a metódust, ami létrehozza, hogy mi történje a gomb megnyomásakor.
                                
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                SBO_Application.StatusBar.SetText(string.Format("{0}-{1}", "SBO_Application_ItemEvent " + pVal.EventType, ex.Message), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
        }
        
    }

}