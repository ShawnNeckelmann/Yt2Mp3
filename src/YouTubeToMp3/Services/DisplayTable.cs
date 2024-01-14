﻿using Spectre.Console;
using YouTubeToMp3.Services.Facade;

namespace YouTubeToMp3.Services;

public class DisplayTable
{
    private const int ColumnIndex_Status = 2;
    private const int ColumnIndex_Title = 1;
    private const int ColumnIndex_URL = 0;

    private const int RefreshDelay = 100;
    private readonly Table _table;
    private readonly Dictionary<Uri, int> _urlRowNumberDictionary = new();
    private bool _processingComplete;


    public DisplayTable()
    {
        _table = new Table().Centered();
        _table.Border(TableBorder.Heavy);


        var urlColumn = new TableColumn("[green]URL[/]")
            .Width(45)
            .Centered();

        var titleColumn = new TableColumn("[fuchsia]Title[/]")
            .Width(75)
            .NoWrap()
            .Centered();


        var statusColumn = new TableColumn("[yellow]Status[/]")
            .Width(45)
            .Centered();

        _table.AddColumns(urlColumn, titleColumn, statusColumn);
        _table.Title = new TableTitle("YouTube to MP3");
        _table.Caption = new TableTitle("Shawn Neckelmann");
    }

    public void AddYouTubeVideo(string uri)
    {
        var asUri = new Uri(uri);
        _table.AddRow($"[link]{asUri}[/]", "Pending", "Pending");
        _urlRowNumberDictionary.Add(asUri, _table.Rows.Count - 1);
    }

    public void Complete(Uri url)
    {
        var rowNumber = RowNumber(url);
        _table.UpdateCell(rowNumber, ColumnIndex_Status, "Complete");
    }

    private void Complete()
    {
        _processingComplete = true;
    }

    private void Remove(Uri uri)
    {
        _urlRowNumberDictionary.Remove(uri);
        if (_urlRowNumberDictionary.Count == 0)
        {
            Complete();
        }
    }

    public async Task Render()
    {
        await AnsiConsole.Live(_table).StartAsync(async context =>
        {
            while (!_processingComplete)
            {
                context.Refresh();
                await Task.Delay(RefreshDelay);
            }
        });
    }

    private int RowNumber(Uri url)
    {
        return _urlRowNumberDictionary[url];
    }

    public void SetException(Uri url, string exceptionMessage)
    {
        var rowNumber = RowNumber(url);
        SetStatus(url, exceptionMessage);

        var cellData = new Text(exceptionMessage, new Style(Color.Red, Color.Black));
        _table.UpdateCell(rowNumber, ColumnIndex_Status, cellData);
    }


    public void SetStatus(Uri url, string status, YouTubeData data)
    {
        var rowNumber = RowNumber(url);
        _table.UpdateCell(rowNumber, ColumnIndex_Title, data.DisplayTitle);
        _table.UpdateCell(rowNumber, ColumnIndex_Status, status);
    }

    public void SetStatus(Uri url, string status)
    {
        var rowNumber = RowNumber(url);
        _table.UpdateCell(rowNumber, ColumnIndex_Status, status);
    }
}