if exist .OpenIDE\prerun.bat (
    .OpenIDE\prerun.bat %*
)
oi.exe %*
if exist .OpenIDE\postrun.bat (
    .OpenIDE\postrun.bat %*
)
