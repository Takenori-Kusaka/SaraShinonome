static class CollisionParts
{
	public const string Head = "head";
	public const string Shoulder = "shoulder";
	public const string Hand = "hand";

	public static string GetCollisionPartsName(string parts)
	{
		switch (parts)
		{
			case Head:
				return "頭";
			case Shoulder:
				return "肩";
			case Hand:
				return "手";
			default:
				return null;
		}
	}
}