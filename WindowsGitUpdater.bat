echo off
echo.
echo This program updates fps-game
echo You must have git added to cmd
echo.

git reset --hard
git clean -f
git pull

echo.
echo Successfully updated!
echo.

pause