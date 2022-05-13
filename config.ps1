$xml = @'
<?xml version="1.0"?>
<Project>
    <PropertyGroup>
        <EtgManagedDirectoryPath Condition="'$(EtgManagedDirectoryPath)' == ''">{{value}}</EtgManagedDirectoryPath>
        <EtgManagedDirectoryPath Condition="!$(EtgManagedDirectoryPath.EndsWith('\'))">$(EtgManagedDirectoryPath)\</EtgManagedDirectoryPath>
    </PropertyGroup>
</Project>
'@

$etgManagedDir = Read-Host "ETG Managed Directory"
Set-Content -Path local.props -Value $($xml -replace "{{value}}", $etgManagedDir)