namespace PhylogeneticApp.Utils;

/// <summary>
/// A progress bar directed for console writing
/// </summary>
public class CountdownProgressBar
{
    private readonly int _totalTicks;
    private readonly int _divisions;
    private int _currentTick;
    private int _lastPercentage;
    
    //ProgressBar console position

    /// <summary>
    /// Creates a new progress bar
    /// </summary>
    /// <param name="totalTicks"> The total number of ticks the progress bar will have</param>
    public CountdownProgressBar(int totalTicks, int divisions)
    {
        _totalTicks = totalTicks;
        _divisions = divisions;
        _currentTick = 0;
        _lastPercentage = 0;
    }

    /// <summary>
    /// Reports the progress of the progress bar
    /// </summary>
    /// <param name="currentTick"> The current tick of the progress bar</param>
    public void Report(int currentTick)
    {
        _currentTick = currentTick;
        int percentage = (int)((float)_currentTick / _totalTicks * 100);
        if (percentage != _lastPercentage)
        {
            _lastPercentage = percentage;
            ConsolePrint(percentage);
        }
    }

    /// <summary>
    /// Prints the progress bar to the console as:
    /// [=====>    ] 50% where the number of = is divisions and > is the current tick percentage
    /// It also ensures that the progress bar is only shown once
    /// </summary>
    private void ConsolePrint(int percentage)
    {
        int currentDivisions = (int)((float)_currentTick / _totalTicks * _divisions);
        Console.Write($"\r[{new string('=', currentDivisions)}{new string(' ', _divisions - currentDivisions)}] {percentage}%");
    }
    
    public void Finish()
    {
        // Remove the progress bar
        Console.Write("\r" + string.Empty);
    }
}