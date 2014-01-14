MvvmNotificationChainer
================

What is it?
-----------
Portable Class Library that simplifies chaining ViewModel property PropertyChanged events.

Why would I want to use it?
---------------------------
Let's say you have a ViewModel property "Sum" which is a calculated property that depends on properties "Int1" and "Int2". Setting values on Int1 and Int2 will raise a PropertyChanged event for themselves, and also for Sum.

Traditionally, the ViewModel properties would look like this:

	private int myInt1;
	public int Int1
	{
		get { return myInt1; }
		set
		{
			myInt1 = value;
			RaisePropertyChanged("Int1");
			RaisePropertyChanged("Sum");
		}
	}

	private int myInt2;
	public int Int2
	{
		get { return myInt2; }
		set
		{
			myInt2 = value;
			RaisePropertyChanged("Int2");
			RaisePropertyChanged("Sum");
		}
	}

	public int Sum
	{
		get { return myInt1 + myInt2; }
	}

What bugs me about this approach is that Int1 and Int2 have to know about Sum. In my opinion, that flow of knowledge should be reversed: only Sum should know that it depends on Int1 and Int2.

So how can we reverse that knowledge? That's the goal of MvvmNotificationChainer:

	protected readonly ChainedNotificationCollection myChainedNotifications = new ChainedNotificationCollection();

	private int myInt1;
	public int Int1
	{
		get { return myInt1; }
		set
		{
			myInt1 = value;
			RaisePropertyChanged("Int1");
		}
	}
	
	private int myInt2;
	public int Int2
	{
		get { return myInt2; }
		set
		{
			myInt2 = value;
			RaisePropertyChanged("Int2");
		}
	}
	
	public int Sum
	{
		get
		{
			myChainedNotifications.Create()
									.Register (cnd => cnd.On (this, () => Example2Int1)
														.On (this, () => Example2Int2)
														.AndCall (RaisePropertyChanged))
									.Finish ();
			
			return myInt1 + myInt2;
		}
	}

Now, with the help of ChainedNotificationCollection, only Sum has to know that it depends on Int1 and Int2.

Here's the ChainedNotificationCollection.Create method:

	/// <summary>
	/// Creates a ChainedNotification for the calling property, or returns an existing instance
	/// </summary>
	/// <param name="chainedPropertyName">Name of the property that depends on other properties</param>
	/// <returns></returns>
	public ChainedNotification Create([CallerMemberName] string chainedPropertyName = null)
	{
		chainedPropertyName.ThrowIfNull("chainedPropertyName");

		ChainedNotification cnd;
		if (!myChainedNotifications.TryGetValue(chainedPropertyName, out cnd))
			cnd = myChainedNotifications[chainedPropertyName] = new ChainedNotification(chainedPropertyName);

		return cnd;
	}

[CallerMemberName] is used so that there is compile-time property name safety w/o using Expression<Func<T>>, and then you don't have to specify the chainedPropertyName parameter. Unfortunately [CallerMemberName] doesn't seem to be PCL-compatible, so this functionality will have to be implemented on the platform-specific project.

Use the ChainedNotification.On<T> methods to supply a lambda Expression to specify which changed properties to watch.

ChainedNotification is smart enough to Register() just once - therefore you can use Expression<Func<T>> to have compile-time property name safety but only pay the performance penalty for the initial call. After Finish() is called, the other methods won't do anything.

ChainedNotification/Collection also implement IDisposable - when disposed, they will unregister event handlers.

Status
------

Initial functionality is done and seems to work as expected.

Implemented Features:
* Performs ProperyChanged for a chained property whenever a watched property changes
* Compile-time property name safety for easy refactoring
* Executes just once per chain, trivial performance hit on initialization

To Dos:
* Write unit tests (I know, I suck at TDD)

Caveats:
* You can probably use Reactive Extensions to solve this problem, but I still can't grok it yet. This just seems a lot more straightfoward, and doesn't require extensive rewriting of existing ViewModels.

Shout-outs
----------

Thanks to [Interknowlogy's PDFx](http://blogs.interknowlogy.com/2013/05/17/pdfx-property-dependency-framework-part-i-introduction-2/) for the inspiration!

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