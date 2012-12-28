$root = "\\Server\Music\Artists\U2"
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
                        @{ $_ = [System.Text.Encoding]::Unicode.GetString($frame.PrivateData.Data) }
                    }
                    "WM/MediaClass*" {
                        @{ $_ = [guid] $frame.PrivateData.Data }
                    }
                    { ($_ -eq "WM/WMContentID") -or ($_ -eq "WM/WMCollectionID") -or ($_ -eq "WM/WMCollectionGroupID") } {
                        @{ $_ = [guid] $frame.PrivateData.Data }
                    }
                    "WM/UniqueFileIdentifier" {
                        # WM/UniqueFileIdentifier is UTF-16 text.
                        @{ $_ = [System.Text.Encoding]::Unicode.GetString($frame.PrivateData.Data) }
                    }
                    default {
                        @{ $_ = $frame.PrivateData.Data }
                    }
                }
            }
    }
