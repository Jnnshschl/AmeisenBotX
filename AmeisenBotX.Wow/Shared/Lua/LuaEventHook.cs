namespace AmeisenBotX.Wow.Shared.Lua
{
    public static class LuaEventHook
    {
        public static string Get(string frame, string table, string handlerFn, string output)
        {
            return @$"
                {output}='['

                function {handlerFn}(self,a,...)
                    table.insert({table},{{time(),a,{{...}}}})
                end

                if {frame}==nil then
                    {table}={{}}
                    {frame}=CreateFrame(""FRAME"")
                    {frame}:SetScript(""OnEvent"",{handlerFn})
                else
                    for b,c in pairs({table})do
                        {output}={output}..'{{'

                        for d,e in pairs(c)do
                            if type(e)==""table""then
                                {output}={output}..'""args"": ['

                                for f,g in pairs(e)do
                                    {output}={output}..'""'..tostring(g)..'""'

                                    if f<=table.getn(e)then
                                        {output}={output}..','
                                    end
                                end

                                {output}={output}..']}}'

                                if b<table.getn({table})then
                                    {output}={output}..','
                                end
                            else
                                if type(e)==""string""then
                                    {output}={output}..'""event"": ""'..e..'"",'
                                else
                                    {output}={output}..'""time"": ""'..e..'"",'
                                end
                            end
                        end
                    end
                end

                {output}={output}..']'
                {table}={{}}
            ";
        }
    }
}