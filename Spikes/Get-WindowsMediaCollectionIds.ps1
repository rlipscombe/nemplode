$root = "\\Server\Music\Artists\Björk"
Get-ChildItem $root -Recurse -Include *.mp3 |
    foreach {
        $result = $_
        
        $file = [TagLib.File]::Create($_)
        $tag = $file.GetTag('Id3v2')
        
        $tag |
            where { $_.Owner -like 'WM/*' } |
            foreach {
                $frame = $_
                switch -wildcard ($frame.Owner) {
                    { ($_ -eq "WM/WMContentID") -or ($_ -eq "WM/WMCollectionID") -or ($_ -eq "WM/WMCollectionGroupID") } {
                        $value = [guid] $frame.PrivateData.Data
                        $result = $result | Add-Member -Name $_ -Value $value -MemberType NoteProperty -PassThru
                    }
                }
            }
            
        $result
    }
