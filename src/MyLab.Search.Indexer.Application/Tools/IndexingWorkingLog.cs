using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MyLab.Log.Dsl;

namespace MyLab.Search.Indexer.Application.Tools;

class IndexingWorkingLog
{
    readonly IDslLogger _log;
    int _totalCount;
    int _totalPages;
    Stopwatch? _stopwatch;
    
    IndexingWorkingLog(IDslLogger log)
    {
        _log = log;
    }

    public static IndexingWorkingLog Start(IDslLogger log)
    {
        var l = new IndexingWorkingLog(log);
        l.StartCore();


        return l;
    }

    public void ReportNextPage(int rowsCount)
    {
        _totalCount += rowsCount;
        _totalPages += 1;
    }

    public void StopAndWriteLog()
    {
        _stopwatch?.Stop();

        if (_totalCount != 0)
        {
            _log.Action("Indexer useful work")
                .AndFactIs("total-count", _totalCount)
                .AndFactIs("total-pages", _totalPages)
                .AndFactIs("elapsed", _stopwatch?.ToString())
                .Write();
        }
        else
        {
            _log.Debug("Indexer useless work").Write();
        }
    }
    
    void StartCore()
    {
        _stopwatch = Stopwatch.StartNew();
    }
}