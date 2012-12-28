$root = "\\Server\Music\Artists\U2\All That You Can't Leave Behind"
Get-ChildItem $root -Recurse -Include *.mp3 |
    foreach {
        $file = [TagLib.File]::Create($_)
        $tag = $file.GetTag('Id3v2')
        
        $tag |
            where { $_.Owner -like 'WM/*' } |
            foreach {
                $frame = $_
                switch -wildcard ($frame.Owner) {
                    "WM/Provider" {
                        # WM/Provider is UTF-16 text.
                        @{ Owner = $frame.Owner; Value = [System.Text.Encoding]::Unicode.GetString($frame.PrivateData.Data) }
                    }
                    "WM/MediaClass*" {
                        @{ Owner = $frame.Owner; Value = [guid] $frame.PrivateData.Data }
                    }
                    { $_ -eq "WM/ContentID" -or $_ -eq "WM/CollectionID" -or $_ -eq "WM/CollectionGroupID" } {
                        [guid] $frame.PrivateData.Data
                    }
                    "WM/UniqueFileIdentifier" {
                        # WM/UniqueFileIdentifier is UTF-16 text.
                        @{ Owner = $frame.Owner; Value = [System.Text.Encoding]::Unicode.GetString($frame.PrivateData.Data) }
                    }
                    default {
                        @{ Owner = $frame.Owner; Length = $frame.PrivateData.Data.Length; Data = $frame.PrivateData.Data }
                    }
                }
            }
    }
