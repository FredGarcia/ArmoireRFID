using ArmoireV3.Entities;

namespace ArmoireV3.Inter
{
    static public class LanguageHelper
    {
        static LanguageHelper()
        {

            // On renseigne Languages.Culture dès que l'on a l'Id du User (dès qu'il passe sa carte)
            // Ceci est fait dans Login.xaml.cs

            Languages.Culture = new System.Globalization.CultureInfo("fr-FR");
            //Languages.Culture = System.Globalization.CultureInfo.CurrentCulture;
        }


        // getters obligatoires afin de lier les textes depuis l'ui et les messagebox

        static public string addDataError
        {
            get { return Languages.addDataError; }
        }

        static public string alertSyncError
        {
            get { return Languages.alertSyncError; }
        }

        static public string articleUpCase
        {
            get { return Languages.articleUpCase; }
        }

        static public string buttonDeconnexion
        {
            get { return Languages.buttonDeconnexion; }
        }


        static public string capMax
        {
            get { return Languages.capMax; }
        }

        static public string close
        {
            get { return Languages.close; }
        }

        static public string connectDBfail
        {
            get { return Languages.connectDBfail; }
        }

        static public string credit
        {
            get { return Languages.credit; }
        }

        static public string curInCab
        {
            get { return Languages.curInCab; }
        }

        static public string cusoon
        {
            get { return Languages.cusoon; }
        }

        static public string dbError
        {
            get { return Languages.dbError; }
        }

        static public string discTip
        {
            get { return Languages.discTip; }
        }

        static public string doorSensor
        {
            get { return Languages.doorSensor; }
        }

        static public string errorOccurred
        {
            get { return Languages.errorOccurred; }
        }

        static public string errorArticleType
        {
            get { return Languages.errorArticleType; }
        }

        static public string evalContent
        {
            get { return Languages.evalContent; }
        }

        static public string failureSync
        {
            get { return Languages.failedSync; }
        }

        static public string globalSyncError
        {
            get { return Languages.globalSyncError; }
        }

        static public string hello
        {
            get { return Languages.hello; }
        }

        static public string inactiveUser
        {
            get { return Languages.inactiveUser; }
        }

        static public string incorLogin
        {
            get { return Languages.incorLogin; }
        }

        static public string insertAlertError
        {
            get { return Languages.insertAlertError; }
        }

        static public string insertDataError
        {
            get { return Languages.insertDataError; }
        }

        static public string insertTagAlertError
        {
            get { return Languages.insertTagAlertError; }
        }

        static public string intru
        {
            get { return Languages.intru; }
        }

        static public string LEDcardError
        {
            get { return Languages.LEDcardError; }
        }

        static public string loadSyncError
        {
            get { return Languages.loadSyncError; }
        }

        static public string loginError
        {
            get { return Languages.loginError; }
        }

        static public string mainWinError
        {
            get { return Languages.mainWinError; }
        }

        static public string max
        {
            get { return Languages.max; }
        }

        static public string myLocker
        {
            get { return Languages.myLocker; }
        }

        static public string mySelect
        {
            get { return Languages.mySelect; }
        }


        static public string nbArtCab
        {
            get { return Languages.nbArtCab; }
        }

        static public string noBadgeReader
        {
            get { return Languages.noBadgeReader; }
        }

        static public string noLEDcard
        {
            get { return Languages.noLEDcard; }
        }

        static public string noRail
        {
            get { return Languages.noRail; }
        }

        static public string notEnoughCredits
        {
            get { return Languages.notEnoughCredits; }
        }

        static public string notinStock1
        {
            get { return Languages.notinStock1; }
        }

        static public string notinStock2
        {
            get { return Languages.notinStock2; }
        }

        static public string notinStock3
        {
            get { return Languages.notinStock3; }
        }

        static public string ongoingService
        {
            get { return Languages.ongoingService; }
        }

        static public string open
        {
            get { return Languages.open; }
        }

        static public string newmap
        {
            get { return Languages.newmap; }
        }

        static public string openCab1minError
        {
            get { return Languages.openCab1minError; }
        }

        static public string openCab30secError
        {
            get { return Languages.openCab30secError; }
        }

        static public string opWithoutRef
        {
            get { return Languages.opWithoutRef; }
        }

        static public string opWithoutSync
        {
            get { return Languages.opWithoutSync; }
        }

        static public string outofstock
        {
            get { return Languages.outofstock; }
        }

        static public string posInCab
        {
            get { return Languages.posInCab; }
        }

        static public string problemTryAgain
        {
            get { return Languages.problemTryAgain; }
        }

        static public string propositionSizeSup
        {
            get { return Languages.propositionSizeSup; }
        }

        static public string quant
        {
            get { return Languages.quant; }
        }

        static public string reloadSyncError
        {
            get { return Languages.reloadSyncError; }
        }

        static public string reqLargerArticleCondition
        {
            get { return Languages.reqLargerArticleCondition; }
        }

        static public string restSyncError
        {
            get { return Languages.restSyncError; }
        }

        static public string returnText
        {
            get { return Languages.returnText; }
        }

        static public string RFIDScanRetraitError
        {
            get { return Languages.RFIDScanRetraitError; }
        }

        static public string scanError
        {
            get { return Languages.scanError; }
        }

        static public string secRemain
        {
            get { return Languages.secRemain; }
        }

        static public string selectOutfit
        {
            get { return Languages.selectOutfit; }
        }

        static public string size
        {
            get { return Languages.size; }
        }

        static public string sizeUpCase
        {
            get { return Languages.sizeUpCase; }
        }

        static public string synchroCabError
        {
            get { return Languages.synchroCabError; }
        }

        static public string synchroInitError
        {
            get { return Languages.synchroInitError; }
        }

        static public string syncInitError
        {
            get { return Languages.syncInitError; }
        }

        static public string systemError
        {
            get { return Languages.systemError; }
        }

        static public string textBoxErreur
        {
            get { return Languages.textBoxErreur; }
        }
        static public string textBoxErreurLocalBDDAvailable
        {
            get { return Languages.textBoxErreurLocalBDDAvailable; }
        }
        static public string textBoxErreurRailAvailable
        {
            get { return Languages.textBoxErreurRailAvailable; }
        }
        static public string textBoxErreurInitialisation
        {
            get { return Languages.textBoxErreurInitialisation; }
        }
        static public string textBoxErreurImpinj
        {
            get { return Languages.textBoxErreurImpinj; }
        }

        static public string textBoxErreurImpinjRestitution
        {
            get { return Languages.textBoxErreurImpinjRestitution; }
        }

        static public string textBoxInsererBadge
        {
            get { return Languages.textBoxInsererBadge; }
        }

        static public string textBoxMagnetoID
        {
            get { return Languages.textBoxMagnetoID; }
        }

        static public string thereR
        {
            get { return Languages.thereR; }
        }

        static public string unhandledErrorOcc
        {
            get { return Languages.unhandledErrorOcc; }
        }

        static public string updateAlertsProblem
        {
            get { return Languages.updateAlertsProblem; }
        }

        static public string updateDataError
        {
            get { return Languages.updateDataError; }
        }

        static public string updateEpcReloadTagError
        {
            get { return Languages.updateEpcReloadTagError; }
        }

        static public string updateEpcRestitutionTagError
        {
            get { return Languages.updateEpcRestitutionTagError; }
        }

        static public string updateError
        {
            get { return Languages.updateError; }
        }

        static public string userSyncError
        {
            get { return Languages.userSyncError; }
        }

        static public string userTimerSyncError
        {
            get { return Languages.userTimerSyncError; }
        }

        static public string validate
        {
            get { return Languages.validate; }
        }

        static public string wait
        {
            get { return Languages.wait; }
        }

        static public string waitDuringScan
        {
            get { return Languages.waitDuringScan; }
        }

        static public string welcome
        {
            get { return Languages.welcome; }
        }

        static public string welIDbox
        {
            get { return Languages.welIDbox; }
        }

        static public string wrongUserType
        {
            get { return Languages.wrongUserType; }
        }

        static public string yourOrder
        {
            get { return Languages.yourOrder; }
        }

        static public string zsyncLoadError
        {
            get { return Languages.zsyncLoadError; }
        }

        static public string zsyncUserError
        {
            get { return Languages.zsyncUserError; }
        }

        // Si nouveau tag, l'exposer en respectant l'ordre alphabétique
    }
}
