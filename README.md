MvvmNotificationChainer
=======================

What is it?
-----------
Portable Class Library that simplifies chaining ViewModel property PropertyChanged events.

Why would I want to use it?
---------------------------
For example, let's say you have a ViewModel property *Cost* which is a calculated property that depends on properties *Quantity* and *Price*. Setting values on *Quantity* and *Price* will raise a `PropertyChanged` event for themselves, and also for *Cost*.

Traditionally, the ViewModel properties would look like this:

```C#
private int myQuantity;
public int Quantity
{
	get { return myQuantity; }
	set
	{
		myQuantity = value;
		RaisePropertyChanged("Quantity");
		RaisePropertyChanged("Cost");
	}
}

private decimal myPrice;
public decimal Price
{
	get { return myPrice; }
	set
	{
		myPrice = value;
		RaisePropertyChanged("Price");
		RaisePropertyChanged("Cost");
	}
}

public decimal Cost
{ get { return Quantity * Price; } }
```

What bugs me about this approach is that *Quantity* and *Price* have to know about *Cost* (Quantity/Price -&gt; Cost). In my opinion, that flow of knowledge should be reversed: only *Cost* should know that it depends on *Quantity* and *Price* (Cost -&gt; Quantity/Price). Indeed, in *Cost*'s getter we can clearly see that calculation already knows about *Quantity* and *Price*. So putting the notification logic in the same place results in no additional conceptual knowledge.

This follows the software engineering principles of increasing [Cohesion](http://en.wikipedia.org/wiki/Cohesion_%28computer_science%29) (the degree to which the elements of a module belong together) and decreasing [Coupling](http://en.wikipedia.org/wiki/Coupling_%28computer_science%29) (specifically, Content Coupling, when one module modifies or relies on the internal workings of another module).

So how can we keep that knowledge grouped properly? Using MvvmNotificationChainer, it can be rewritten like this:

```C#
private int myQuantity;
public int Quantity
{
	get { return myQuantity; }
	set
	{
		myQuantity = value;
		RaisePropertyChanged("Quantity");
	}
}

private decimal myPrice;
public decimal Price
{
	get { return myPrice; }
	set
	{
		myPrice = value;
		RaisePropertyChanged("Price");
	}
}

public decimal Cost
{
	get
	{
		myNotificationChainManager.CreateOrGet()
		                          .Register (cn => cn.On (() => Quantity)
		                                             .On (() => Price)
		                                             .Finish ());
		
		return Quantity * Price;
	}
}

protected readonly NotificationChainManager myNotificationChainManager = new NotificationChainManager();

public MyViewModel ()
{
	myNotificationChainManager.SetDefaultNotifyingObject (this);
	myNotificationChainManager.AddDefaultCall ((notifyingProperty, dependentProperty) => RaisePropertyChanged (dependentProperty));
}
```

Now, by using the **MvvmNotificationChainer** library, only *Cost* knows that it depends on *Quantity* and *Price*. *Quantity* and *Price* are completely independent and unaware of *Cost*.

Here's the `NotificationChainManager.CreateOrGet()` method:

```C#
/// <summary>
/// Creates a new NotificationChain for the calling property, or returns an existing instance
/// </summary>
/// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
/// <returns></returns>
public NotificationChain CreateOrGet ([CallerMemberName] string dependentPropertyName = null)
{
    dependentPropertyName.ThrowIfNull ("dependentPropertyName");

    NotificationChain chain;
    if (!myChains.TryGetValue (dependentPropertyName, out chain))
    {
        chain = myChains[dependentPropertyName] = new NotificationChain (dependentPropertyName);
        chain.AndSetDefaultNotifyingObject (myDefaultNotifyingObject, myDefaultAddEventAction, myDefaultRemoveEventAction);
        foreach (var callback in myDefaultCallbacks)
            chain.AndCall (callback);
    }

    return chain;
}
```

The manager helps to provide, consolidate, and configure multiple `NotificationChain`s for a parent object. Note that this doesn't have to be a ViewModel, it's really for any class that implements or depends on `INotifyPropertyChanged`.

`[CallerMemberName]` is used so that there is compile-time property name safety w/o using `Expression<Func<T>>`, and then you don't have to specify the dependentPropertyName parameter.

The `NotificationChain.On` methods to supply a lambda Expression to specify which changed properties to watch.

Each chain is smart enough to call `Register()` just once - therefore you can use `Expression<Func<T>>`; to have compile-time property name safety but only pay the performance penalty for the initial call. After `Finish()` is called, the other methods won't do anything.

Usage
-----

Now let's analyze the method calls line by line:

```C#
/*NotificationChainManager*/ .SetDefaultNotifyingObject (this);
```

When a new chain is created, it will be configured to listen to the current class (that implements `INotifyPropertyChanged`)

```C#
/*NotificationChainManager*/ .AddDefaultCall ((notifyingProperty, dependentProperty) => RaisePropertyChanged (dependentProperty));
```

Also when a new chain is created, and when a watched property changes, have it call the current class' RaisePropertyChanged event.

```C#
/*NotificationChainManager*/ .CreateOrGet()
```

Creates or gets an existing chain for the calling dependent property

```C#
/*NotificationChain*/ .Register (cn => cn // ...
```

Configures the chain with the given `Action<NotificationChain>`, which executes unless `Finish()` has been called.

```C#
/*NotificationChain*/ .On (() => Quantity)
```

Tells the chain to watch for *Quantity* to change

```C#
/*NotificationChain*/ .On (() => Price)
```

Tells the chain to watch for *Price* to change

```C#
/*NotificationChain*/ .Finish ()
````

Tells the chain that it is done being configured, and not to allow any further configuration changes.

Deep Observation
----------------

`NotificationChain` can also observe "deep" - for example:

```C#
private User myUser;
public User User
{
	get { return myUser; }
	set
	{
		myUser = value;
		RaisePropertyChanged("User");
	}
}

public String UserFirstName
{
	get
	{
		myNotificationChainManager.CreateOrGet()
		                          .Register (cn => cn.On (() => User, u => u.FirstName)
		                                             .Finish ());
		
		return User != null ? User.FirstName : String.Empty;
	}
}
```

UserFirstName will notify when User or User.FirstName notifies. Currently depth of 4 is supported, but i'm working on refactoring to support more depths.

Integrate with MVVM ICommands
-----------------------------

`NotificationChain` can also be used to support Commands - for instance, calling Prism's `DelegateCommand.RaiseCanExecuteChanged` when a watched property changes

```C#
// this is a notifying property that shouldn't have to know about DoSomethingCommand
private bool myHasInternetConnection;
public bool HasInternetConnection
{
	get { return myHasInternetConnection; }
	set
	{
		myHasInternetConnection = value;
		RaisePropertyChanged ("HasInternetConnection");
	}
}

public MyViewModel ()
{
	DoSomethingCommand = new DelegateCommand (DoSomething, CanDoSomething);
	// or you could use MvvmCommandWirer :)
}

public DelegateCommand DoSomethingCommand
{ get; private set; }

private bool CanDoSomething ()
{
	myNotificationChainManager.CreateOrGet (() => DoSomethingCommand) // supply Expression since we're calling from a method
	                          .Register (cn => cn.On (() => HasInternetConnection)
	                                             .AndClearCalls () // clear default calls
	                                             .AndCall (DoSomethingCommand.RaiseCanExecuteChanged)
	                                             .Finish ());

    return HasInternetConnection;
}

private void DoSomething ()
{
	// does something
}
```

We can eliminate Content Coupling by keeping knowledge of DoSomethingCommand away from HasInternetConnection and making DoSomethingCommand code contain the relationship to HasInternetConnection (which it already does).

Summary
-------

So with **MvvmNotificationChainer**, you can reduce coupling by keeping the flow of knowledge one way - from dependent to source properties - for both Properties and Commands.

Status
------

Initial functionality is done and seems to work as expected.

Implemented Features:

* Performs PropertyChanged for a dependent property whenever a watched property changes
* Supports notification chains 4 levels deep
* Compile-time property name safety for easy refactoring
* Creates & registers just once per chain, trivial performance hit on initialization
* Bolt-on code: No need to change base classes, or extensively modify existing ViewModels

To Dos:

* Write unit tests (I know, I suck at TDD)
* Support for observing `NotifyCollectionChangedEventHandler`

Caveats:

* You can probably use Reactive Extensions to solve this problem, but I still can't grok it yet. `MvvmNotificationChainer` just seems a lot more straightforward, and doesn't require extensive rewriting of existing ViewModels.

Acknowledgements
----------------

Thanks to [Interknowlogy's PDFx](http://blogs.interknowlogy.com/2013/05/17/pdfx-property-dependency-framework-part-i-introduction-2/) for the inspiration! I might've gone with their library but it looked like I would've needed to rewrite my classes to implement another interface/base class.

License
-------

The MIT License (MIT)

Copyright (c) 2014 PhilChuang.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
