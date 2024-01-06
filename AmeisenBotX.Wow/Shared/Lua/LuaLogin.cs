namespace AmeisenBotX.Wow.Shared.Lua
{
    public static class LuaLogin
    {
        public static string Get(string user, string pass, string realm, int characterslot)
        {
            // CharacterSelect_EnterWorld() got replaced by CharSelectEnterWorldButton:Click() for
            // whetever reason, the mop client freezes if we call this directly
            return @$"
                if AccountLoginUI then
                    AccountLoginUI:Show()
                end
                if ServerAlertFrame and ServerAlertFrame:IsShown() then
                    ServerAlertFrame:Hide()
                elseif ConnectionHelpFrame and ConnectionHelpFrame:IsShown() then
                    ConnectionHelpFrame:Hide()
                    AccountLoginUI:Show()
                elseif CinematicFrame and CinematicFrame:IsShown() then
                    StopCinematic()
                elseif TOSFrame and TOSFrame:IsShown() then
                    TOSAccept:Enable()
                    TOSAccept:Click()
                elseif ScriptErrors and ScriptErrors:IsShown() then
                    ScriptErrors:Hide()
                elseif GlueDialog and GlueDialog:IsShown() then
                    if GlueDialog.which == ""OKAY"" then
                        GlueDialogButton1:Click()
                    end
                elseif CharCreateRandomizeButton and CharCreateRandomizeButton:IsVisible() then
                    CharacterCreate_Back()
                elseif RealmList and RealmList:IsVisible() then
                    for a = 1, #GetRealmCategories() do
                        local found = false
                        for b = 1, GetNumRealms() do
                            if string.lower(GetRealmInfo(a, b)) == string.lower(""{realm}"") then
                                ChangeRealm(a, b)
                                RealmList: Hide()
                                found = true
                                break
                            end
                        end
                        if found then
                            break
                        end
                    end
                elseif CharacterSelectUI and CharacterSelectUI:IsVisible() then
                    if string.find(string.lower(GetServerName()), string.lower(""{realm}"")) then
                        CharacterSelect_SelectCharacter({characterslot + 1})
                        CharSelectEnterWorldButton:Click()
                    elseif RealmList and not RealmList:IsVisible() then
                         CharSelectChangeRealmButton:Click()
                    end
                elseif AccountLoginUI and AccountLoginUI:IsVisible() then
                    DefaultServerLogin(""{user}"", ""{pass}"")
                end
            ";
        }
    }
}