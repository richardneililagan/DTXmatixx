using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace FDK.XML
{
	/// <summary>
	///		XmlSerialize 可能な Dictionary。
	/// </summary>
	/// <remarks>
	///		通常の Dictionary, List などは XmlSerialize できないので作成。
	/// </remarks>
	public class SerializableDictionary<Tkey, Tvalue> : Dictionary<Tkey, Tvalue>, IXmlSerializable
	{
		public class KeyValue
		{
			public Tkey Key { get; set; }
			public Tvalue Value { get; set; }

			public KeyValue()
			{
			}
			public KeyValue( Tkey key, Tvalue value )
			{
				this.Key = key;
				this.Value = value;
			}
		}

		public XmlSchema GetSchema()
			=> null;

		public void ReadXml( XmlReader reader )
		{
			var s = new XmlSerializer( typeof( KeyValue ) );

			reader.Read();

			while( XmlNodeType.EndElement != reader.NodeType )
			{
				if( s.Deserialize( reader ) is KeyValue kv )
				{
					this.Add( kv.Key, kv.Value );
				}
			}

			reader.Read();
		}

		public void WriteXml( XmlWriter writer )
		{
			var s = new XmlSerializer( typeof( KeyValue ) );

			foreach( var key in Keys )
			{
				s.Serialize( writer, new KeyValue( key, this[ key ] ) );
			}
		}
	}
}
