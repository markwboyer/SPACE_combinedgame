
# Go to your repo root
cd C:\Users\markw\GitHub_Cdrive\SPACE_combinedgame

# Get all modified and untracked files
$files = git ls-files -o -m

# Set batch size
$batchSize = 2000
$counter = 0

while ($files.Count -gt 0) {
    # Take the next batch
    $batch = $files[0..([Math]::Min($batchSize, $files.Count) - 1)]

    # Stage the batch
    foreach ($file in $batch) {
        git add -f -- $file
    }

    # Commit the batch
    git commit -m "Partial commit $($counter + 1)"

    # Optional: Push after each commit (or comment this out and push once at the end)
    git push origin main

    # Remove processed files from the list
    $files = $files[$batchSize..($files.Count - 1)]

    $counter++
}
