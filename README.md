MvvmNotificationChainer
=======================

What is it?
-----------
Portable Class Library that simplifies chaining ViewModel property PropertyChanged events.

Why would I want to use it?
---------------------------
Let's say you have a ViewModel property "Cost" which is a calculated property that depends on properties "Quantity" and "Price". Setting values on Quantity and Price will raise a PropertyChanged event for themselves, and also for Cost.

Traditionally, the ViewModel properties would look like this:

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

What bugs me about this approach is that Quantity and Price have to know about Cost. In my opinion, that flow of knowledge should be reversed: only Cost should know that it depends on Quantity and Price. Indeed, in Cost's getter we can clearly see that it depends on Quantity and Price.

So how can we keep that knowledge grouped together? That's the goal of MvvmNotificationChainer:

	protected readonly ChainedNotificationManager myChainedNotifications = new ChainedNotificationManager();

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
			myChainedNotifications.Create()
									.Register (cn => cn.On (this, () => Quantity)
														.On (this, () => Price)
														.AndCall ((notifyingProperty, dependentProperty) => RaisePropertyChanged (dependentProperty))
														.Finish ());
			
			return Quantity + Price;
		}
	}

Now, with the help of ChainedNotificationManager, only Cost has to know that it depends on Quantity and Price.

Here's the ChainedNotificationManager.Create method:

	/// <summary>
	/// Creates a ChainedNotification for the calling property, or returns an existing instance
	/// </summary>
	/// <param name="dependentPropertyName">Name of the property that depends on other properties</param>
	/// <returns></returns>
	public ChainedNotification Create([CallerMemberName] string dependentPropertyName = null)
	{
		dependentPropertyName.ThrowIfNull("dependentPropertyName");

		ChainedNotification cnd;
		if (!myChainedNotifications.TryGetValue(dependentPropertyName, out cnd))
			cnd = myChainedNotifications[dependentPropertyName] = new ChainedNotification(dependentPropertyName);

		return cnd;
	}

[CallerMemberName] is used so that there is compile-time property name safety w/o using Expression&lt;Func&lt;T&gt;&gt;, and then you don't have to specify the dependentPropertyName parameter.

Use the ChainedNotification.On&lt;T&gt; methods to supply a lambda Expression to specify which changed properties to watch.

ChainedNotification is smart enough to Register() just once - therefore you can use Expression&lt;Func&lt;T&gt;&gt; to have compile-time property name safety but only pay the performance penalty for the initial call. After Finish() is called, the other methods won't do anything.

ChainedNotification can also go "deep" - for example:

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
			myChainedNotifications.Create()
									.Register (cn => cn.On (this, () => User, u => u.FirstName)
														.AndCall ((notifyingProperty, dependentProperty) => RaisePropertyChanged (dependentProperty))
														.Finish ());
			
			return User != null ? User.FirstName : String.Empty;
		}
	}

UserFirstName will notify when User or User.FirstName notifies. Currently only depth of 1 is supported, but i'm planning on supporting multiple depths.

ChainedNotification/Manager also implement IDisposable - when disposed, they will unregister event handlers.

Status
------

Initial functionality is done and seems to work as expected.

Implemented Features:
* Performs PropertyChanged for a dependent property whenever a watched property changes
* Compile-time property name safety for easy refactoring
* Creates & registers just once per chain, trivial performance hit on initialization
* Bolt-on code: No need to change base classes, or extensively modify existing ViewModels

To Dos:
* Write unit tests (I know, I suck at TDD)
* Support for NotifyCollectionChangedEventHandler

Caveats:
* You can probably use Reactive Extensions to solve this problem, but I still can't grok it yet. MvvmNotificationChainer just seems a lot more straightfoward, and doesn't require extensive rewriting of existing ViewModels.

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