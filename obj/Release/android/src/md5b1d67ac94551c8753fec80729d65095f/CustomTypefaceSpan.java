package md5b1d67ac94551c8753fec80729d65095f;


public class CustomTypefaceSpan
	extends android.text.style.TypefaceSpan
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_updateDrawState:(Landroid/text/TextPaint;)V:GetUpdateDrawState_Landroid_text_TextPaint_Handler\n" +
			"n_updateMeasureState:(Landroid/text/TextPaint;)V:GetUpdateMeasureState_Landroid_text_TextPaint_Handler\n" +
			"";
		mono.android.Runtime.register ("Totem.CustomTypefaceSpan, Totem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", CustomTypefaceSpan.class, __md_methods);
	}


	public CustomTypefaceSpan (java.lang.String p0) throws java.lang.Throwable
	{
		super (p0);
		if (getClass () == CustomTypefaceSpan.class)
			mono.android.TypeManager.Activate ("Totem.CustomTypefaceSpan, Totem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "System.String, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0 });
	}


	public void updateDrawState (android.text.TextPaint p0)
	{
		n_updateDrawState (p0);
	}

	private native void n_updateDrawState (android.text.TextPaint p0);


	public void updateMeasureState (android.text.TextPaint p0)
	{
		n_updateMeasureState (p0);
	}

	private native void n_updateMeasureState (android.text.TextPaint p0);

	java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
