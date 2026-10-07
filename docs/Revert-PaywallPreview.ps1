[CmdletBinding(SupportsShouldProcess = $true)]
param()

$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskBackupRoot = Join-Path $PSScriptRoot 'paywall-preview-backup'
$taskManifest = Get-Content -LiteralPath (Join-Path $taskBackupRoot 'manifest.json') -Raw | ConvertFrom-Json

function Resolve-WorkspaceFile([string] $relativePath) {
    $resolvedPath = [IO.Path]::GetFullPath((Join-Path $taskRoot $relativePath))
    if (-not $resolvedPath.StartsWith($taskRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path outside this workspace: $relativePath"
    }
    return $resolvedPath
}

# PAYWALL PREVIEW: preflight every file before restoring anything. Later user edits are never overwritten.
foreach ($taskEntry in $taskManifest) {
    $taskTarget = Resolve-WorkspaceFile $taskEntry.Path
    if (-not (Test-Path -LiteralPath $taskTarget)) { throw "Missing file: $($taskEntry.Path). Revert stopped before making changes." }
    if ((Get-FileHash -LiteralPath $taskTarget -Algorithm SHA256).Hash -ne $taskEntry.ImplementedHash) {
        throw "Later edits found in $($taskEntry.Path). Revert stopped; save those edits before reverting manually."
    }
    if ($taskEntry.OriginalHash) {
        $taskBackup = Resolve-WorkspaceFile ('docs/paywall-preview-backup/' + $taskEntry.Path)
        if ((Get-FileHash -LiteralPath $taskBackup -Algorithm SHA256).Hash -ne $taskEntry.OriginalHash) {
            throw "Backup verification failed: $($taskEntry.Path)"
        }
    }
}

foreach ($taskEntry in $taskManifest) {
    $taskTarget = Resolve-WorkspaceFile $taskEntry.Path
    if ($taskEntry.OriginalHash) {
        if ($PSCmdlet.ShouldProcess($taskTarget, 'Restore original file before paywall preview')) {
            Copy-Item -LiteralPath (Resolve-WorkspaceFile ('docs/paywall-preview-backup/' + $taskEntry.Path)) -Destination $taskTarget
        }
    } elseif ($PSCmdlet.ShouldProcess($taskTarget, 'Remove file added for paywall preview')) {
        # Only exact, verified files added by this change are removed; no recursive deletion.
        Remove-Item -LiteralPath $taskTarget
    }
}
Write-Output 'Revert processed. Rebuild the app after restoring. Backups and documentation remain available.'
