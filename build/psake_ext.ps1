function Generate-Assembly-Info
{
param(
	[string]$copyright, 
	[string]$version,
	[string]$company,
	[string]$file = $(throw "file is a required parameter.")
)
  $asmInfo = "using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: CLSCompliantAttribute(false)]
[assembly: ComVisibleAttribute(false)]
[assembly: AssemblyCompanyAttribute(""$company"")]
[assembly: AssemblyCopyrightAttribute(""$copyright"")]
[assembly: AssemblyVersionAttribute(""$version"")]
[assembly: AssemblyInformationalVersionAttribute(""$version"")]
[assembly: AssemblyFileVersionAttribute(""$version"")]
[assembly: AssemblyDelaySignAttribute(false)]
"

	$dir = [System.IO.Path]::GetDirectoryName($file)
	if ([System.IO.Directory]::Exists($dir) -eq $false)
	{
		Write-Host "Creating directory $dir"
		[System.IO.Directory]::CreateDirectory($dir)
	}
	Write-Host "Generating assembly info file: $file"
	out-file -filePath $file -encoding UTF8 -inputObject $asmInfo
}

function Generate-Nuspec-File 
{
param( 
	[string]$version,
	[string]$id,
	[string]$file = $(throw "file is a required parameter."),
	[string[]]$dependencies
	
)
  $contents = "<?xml version=""1.0""?>
<package xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <metadata xmlns=""http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"">
    <id>$id</id>
    <version>$version</version>
    <authors>Maarten Balliauw</authors>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <description>MvcSiteMapProvider is a SiteMapProvider implementation for the ASP.NET MVC framework. The project is hosted on http://mvcsitemap.codeplex.com.</description>
    <summary>MvcSiteMapProvider is a SiteMapProvider implementation for the ASP.NET MVC framework. The project is hosted on http://mvcsitemap.codeplex.com.</summary>
    <language>en-US</language>
    <tags>mvc sitemap menu breadcrumb navigation</tags>
    <projectUrl>http://github.com/maartenba/MvcSiteMapProvider</projectUrl>
    <iconUrl>http://download.codeplex.com/Project/Download/FileDownload.aspx?ProjectName=mvcsitemap&amp;DownloadId=196029</iconUrl>"
	
	if ($dependencies.Length -gt 0) {
		$contents = "$contents
	<dependencies>";

	foreach ($dependency in $dependencies) {
		$contents = "$contents
		<dependency id=`"$dependency`" />";
	}

		$contents = "$contents
	</dependencies>";
	}
	
	$contents = "$contents
  </metadata>
</package>
"

	$dir = [System.IO.Path]::GetDirectoryName($file)
	if ([System.IO.Directory]::Exists($dir) -eq $false)
	{
		Write-Host "Creating directory $dir"
		[System.IO.Directory]::CreateDirectory($dir)
	}
	Write-Host "Generating nuspec file: $file"
	out-file -filePath $file -encoding UTF8 -inputObject $contents
}