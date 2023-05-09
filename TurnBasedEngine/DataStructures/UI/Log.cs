using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[TypeIndex(73)]
public abstract class Log : Model {
    protected static readonly bool DEBUGGING = false;
    public static Log Text(string content) {
        if (LogStack.ActiveStack.Discarding) {
            return null;
        }
        using (var wrapper = LogScope.Inline()) {
            var l = TextLog.Create(content, LogStack.ActiveStack.TopOfStackRelationship);
            LogStack.ActiveStack.NestLoggable(l);
            if (DEBUGGING) {
                Debug.Log(content);
            }
            return l;
        }
    }
    // Logs are inherently decoupled from the things they need to wait for.  So wait for everything.
    public override Dependency GetDependencies() => Dependency.All;
}
[TypeIndex(74)]
// This is a fake log that links the worldview log transform to logs
public class WrapperLog : PopoutNestedLog {
}
[TypeIndex(75)]
public class TextLog : Log, ITooltipParent<SList<Log>>, ILatent {
    public SList<Log> TooltipChildren { get => Get<SList<Log>>(1); set => Set(1, value); }
    public SLong ModelRelationship { get => Get<SLong>(2); set => Set(2, value); }
    public SString TextContent { get => Get<SString>(3); set => Set(3, value); }
    public static Log Create(string content, ulong modelRelationship) {
        if (string.IsNullOrEmpty(content)) {
            return null;
        }
        var log = new TextLog();
        log.Setup();
        log.TextContent = content;
        log.TooltipChildren = new SList<Log>();
        log.ModelRelationship = modelRelationship;
        log.CreateLatentView<TextLogView, TextLog>();
        return log;
    }
}

[TypeIndex(76)]
public class InlineNestedLog : Log, ITooltipParent<SList<Log>>, ILatent {
    public SList<Log> TooltipChildren { get => Get<SList<Log>>(1); set => Set(1, value); }
    public SLong ModelRelationship { get => Get<SLong>(2); set => Set(2, value); }

    public static InlineNestedLog Create(ulong modelRelationship) {
        var log = new InlineNestedLog();
        log.Setup();
        log.TooltipChildren = new SList<Log>();
        log.ModelRelationship = modelRelationship;
        log.CreateLatentView<InlineNestedLogView, InlineNestedLog>();
        return log;
    }
}

[TypeIndex(77)]
public class PopoutNestedLog : Log, ITooltipParent<SList<Log>>, ILatent {
    public SList<Log> TooltipChildren { get => Get<SList<Log>>(1); set => Set(1, value); }
    public SLong ModelRelationship { get => Get<SLong>(2); set => Set(2, value); }
    public InlineNestedLog TopLine { get => Get<InlineNestedLog>(3); set => Set(3, value); }
    public SBool Relevant { get => Get<SBool>(4); set => Set(4, value); }
    public static PopoutNestedLog Create(string content, ulong modelRelationship) {
        var log = new PopoutNestedLog();
        log.Setup();
        log.ModelRelationship = modelRelationship;
        log.TooltipChildren = new SList<Log>();
        var topLine = InlineNestedLog.Create(new SInt2(log.Id, 3));
        topLine.TooltipChildren.Add(TextLog.Create(content, topLine.ChildRelationship()));
        log.TopLine = topLine;
        log.Relevant = true;
        log.CreateLatentView<PopoutNestedLogView, PopoutNestedLog>();
        return log;
    }
}

[TypeIndex(78)]
public class LogStack : SList<Log> {
    protected static Stack<LogStack> activeStacks;
    public static LogStack ActiveStack {
        get {
            if (activeStacks == null) {
                activeStacks = new Stack<LogStack>();
            }
            if (!activeStacks.Any()) {
                return null;
            }
            return activeStacks.Peek();
        }
        set {
            if (activeStacks == null) {
                activeStacks = new Stack<LogStack>();
            }
            if (value == null) {
                activeStacks.Pop();
            }
            else {
                activeStacks.Push(value);
            }
        }
    }
    protected LogStack() { }
    public static LogStack Create() {
        var Logs = new LogStack();
        Logs.Add(new WrapperLog() { TooltipChildren = new SList<Log>() });
        return Logs;
    }
    internal int DiscardRefCount;
    internal bool Discarding => DiscardRefCount > 0;
    internal Log TopOfStack {
        get {
            return innerList[innerList.Count - 1];
        }
    }
    internal ulong TopOfStackRelationship {
        get {
            // All the wrapper logs have their sublogs on key 1
            return new SInt2(innerList[innerList.Count - 1].Id, 1);
        }
    }
    public void NestLoggable(Log log) {
        if (Discarding) {
            return;
        }
        (TopOfStack as ITooltipParent<SList<Log>>).TooltipChildren.Add(log);
    }
    public void OpenScope(Log log) {
        if (log == null) {
            return;
        }
        NestLoggable(log);
        innerList.Add(log);
    }
    public void CloseScope(Log log) {
        if (log == null) {
            return;
        }
        Assert.That(TopOfStack == log, "Tried to pop a log stack that wasn't on top!");
        innerList.Remove(log);
    }
    public void CloseAllOpenScopes() {
        // remove everything but the wrapper
        innerList.RemoveRange(1, innerList.Count - 1);
    }
    public ulong RootRelationship { get => new SInt2(innerList[0].Id.Value, 1); }
}

public class LogScope : IDisposable {
    public Log EnclosingLog;
    public static LogScope Popup(string content) {
        // Technically, this is a bit odd, since you could dispose the last discard scope while you have this log scope open.
        // The solution: don't do that.
        if (LogStack.ActiveStack.Discarding) {
            return null;
        }
        return new LogScope(PopoutNestedLog.Create(content, LogStack.ActiveStack.TopOfStackRelationship));
    }
    public static LogScope Inline() {
        if (LogStack.ActiveStack.Discarding) {
            return null;
        }
        if (LogStack.ActiveStack?.TopOfStack?.GetType() == typeof(InlineNestedLog)) {
            return null;
        }
        return new LogScope(InlineNestedLog.Create(LogStack.ActiveStack.TopOfStackRelationship));
    }
    public LogScope(Log relevantLog) {
        this.EnclosingLog = relevantLog;
        LogStack.ActiveStack.OpenScope(relevantLog);
    }
    public void Dispose() {
        LogStack.ActiveStack.CloseScope(EnclosingLog);
    }
}
public class LogDiscardScope : IDisposable {
    public LogDiscardScope() {
        if (LogStack.ActiveStack != null) {
            LogStack.ActiveStack.DiscardRefCount++;
        }
    }
    public void Dispose() {
        if (LogStack.ActiveStack != null) {
            LogStack.ActiveStack.DiscardRefCount--;
        }
    }
}