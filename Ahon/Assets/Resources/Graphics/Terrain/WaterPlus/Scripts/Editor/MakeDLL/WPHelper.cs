//#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using WaterPlusEditorInternal;

using System.IO;

public enum WPColorChannels {
	r = 0,
	g = 1,
	b = 2,
	a = 3,
	rgb = 4
}

public enum WPBlurType {
	gaussian = 0,
	expand,
	box
}

public enum WPGradientType {
	linear = 0,
	oneMinusSqr = 1,
	sqrOfOneMinusG = 2
}

public enum WPFilteringMethod {
	bilinear = 0,
	bicubic
}

public class WPGrayscaleImage {
	private byte[] pixels;
	public int width;
	public int height;
	
	public WPGrayscaleImage(Texture2D _texture, WPColorChannels _channel) {
		width = _texture.width;
		height = _texture.height;
		
		pixels = new byte[width * height];
		
		Color[] srcPixels = _texture.GetPixels();
		
		for (int i = 0; i < width * height; i++) {
			switch (_channel) {
				default: case WPColorChannels.r:
					pixels[i] = (byte) (srcPixels[i].r * 255.0f);
					break;
				
				case WPColorChannels.g:
					pixels[i] = (byte) (srcPixels[i].g * 255.0f);
					break;
				
				case WPColorChannels.b:
					pixels[i] = (byte) (srcPixels[i].b * 255.0f);
					break;
				
				case WPColorChannels.a:
					pixels[i] = (byte) (srcPixels[i].a * 255.0f);
					break;
			}
		}
	}
	
	public WPGrayscaleImage(int _width, int _height, byte[] _pixels) {
		width = _width;
		height = _height;
		
		SetPixels(_pixels);
	}
	
	public WPGrayscaleImage(int _width, int _height, int[] _pixels) {
		width = _width;
		height = _height;
		
		SetPixels(_pixels);
	}
	
	public byte[] GetPixels() {
		byte[] pixelsCopy = new byte[width * height];
		
		for (int i = 0; i < width * height; i++) {
			pixelsCopy[i] = pixels[i];	
		}
		
		return pixelsCopy;
	}
	
	public void SetPixels(byte[] _pixels) {
		pixels = new byte[width * height];
		
		for (int i = 0; i < width * height; i++) {
			pixels[i] = _pixels[i];	
		}
	}
	
	public void SetPixels(int[] _pixels) {
		pixels = new byte[width * height];
		
		for (int i = 0; i < width * height; i++) {
			pixels[i] = (byte)_pixels[i];	
		}
	}
	
	public static Texture2D MakeTexture2D(WPGrayscaleImage _r, WPGrayscaleImage _g, WPGrayscaleImage _b, WPGrayscaleImage _a) {
		bool doDimensionsMatch = true;
		
		if (_r != null && _g != null) {
			if (_r.width != _g.width || _r.height != _g.height)
				doDimensionsMatch = false;
		}
		
		if (_g != null && _b != null) {
			if (_g.width != _b.width || _g.height != _b.height)
				doDimensionsMatch = false;
		}
		
		if (_b != null && _r != null) {
			if (_b.width != _r.width || _b.height != _r.height)
				doDimensionsMatch = false;
		}
		
		if (!doDimensionsMatch) {
			Debug.LogError("Cannot make a texture - dimensions mismatch.");
			_g = null;
			_b = null;
			_a = null;
			//return null;
		}
			
		Texture2D resultTexture = new Texture2D(_r.width, _r.height, TextureFormat.ARGB32, false);
		Color[] resPixels = new Color[_r.width * _r.height];
		
		byte[] rPixels = null;
		byte[] gPixels = null;
		byte[] bPixels = null;
		byte[] aPixels = null;
		
		if (_r != null)
			rPixels = _r.GetPixels();
		
		if (_g != null)
			gPixels = _g.GetPixels();
		
		if (_b != null)
			bPixels = _b.GetPixels();
		
		if (_a != null)
			aPixels = _a.GetPixels();
		
		for (int i = 0; i < _r.width * _r.height; i++) {
			if (_r != null)
				resPixels[i].r = (float)rPixels[i] / 255.0f;
			else
				resPixels[i].r = 0.0f;
			
			
			if (_g != null)
				resPixels[i].g = (float)gPixels[i] / 255.0f;
			else
				resPixels[i].g = 0.0f;
			
			
			if (_b != null)
				resPixels[i].b = (float)bPixels[i] / 255.0f;
			else
				resPixels[i].b = 0.0f;
			
			
			if (_a != null)
				resPixels[i].a = (float)aPixels[i] / 255.0f;
			else
				resPixels[i].a = 1.0f;
		}
		
		resultTexture.SetPixels( resPixels );
		resultTexture.Apply();
		
		return resultTexture;
	}
	
	public static WPGrayscaleImage ValueImage(int _width, int _height, byte _value) {
		byte[] _pixels = new byte[_width * _height];
		
		for (int i = 0; i < _width * _height; i++) {
			_pixels[i] = _value;
		}
		
		return new WPGrayscaleImage(_width, _height, _pixels);
	}
	
	public static byte[] ValuePixels(int _width, int _height, byte _value) {
		byte[] _pixels = new byte[_width * _height];
		
		for (int i = 0; i < _width * _height; i++) {
			_pixels[i] = _value;
		}
		
		return _pixels;
	}
	
	public static int[] ValuePixelsInt(int _width, int _height, int _value) {
		int[] _pixels = new int[_width * _height];
		
		for (int i = 0; i < _width * _height; i++) {
			_pixels[i] = _value;
		}
		
		return _pixels;
	}
}

static public class WPHelper {
	public static string waterSystemPath = "Assets/WaterPlus/";
	//static HelperGameObject helperGameObject = new HelperGameObject();
		
	//const int terrainLayer = 12;
	//const int terrainLayerMask = 1 << terrainLayer;
	
	#region Lightmapping Helpers
	static public float Max (float a, float b, float c)
	{
		float max = a;

		if (b > max)
			max = b;

		if (c > max)
			max = c;

		return max;
	}

	static public float Min (float a, float b, float c)
	{
		float min = a;
		
		if (b < min)
			min = b;
		
		if (c < min)
			min = c;
		
		return min;
	}
	
	static public void UVToVertex(Vector2 _uv, WPMesh _mesh, Vector2[] _UVs, out bool vertexFound, out Vector3 vertexPos) {
		int[] triangles = _mesh.triangles;
		Vector3[] vertices = _mesh.vertices;
		
		int[] triangleFoundVertices = new int[3];
		
		bool triangleFound = false;
		
		int uvsLength = _UVs.Length;
		
		//Find to what triangle the UV belongs to.
		for (int i = 0; i < triangles.Length; i += 3) {
			if ( triangles[i] >= uvsLength || triangles[i + 1] >= uvsLength || triangles[i + 2] >= uvsLength)
				continue;
			
			Vector2 uv1 = _UVs[ triangles[i] ];
			Vector2 uv2 = _UVs[ triangles[i + 1] ];
			Vector2 uv3 = _UVs[ triangles[i + 2] ];

			//Console.WriteLine("uvs: " + uv1.ToString() + " " + uv2.ToString() + " " + uv3.ToString() + " point: " + _uv.ToString() );

			if ( IsPointWithinTriangle( uv1, uv2, uv3, _uv ) ) {
				triangleFoundVertices[0] = triangles[i];
				triangleFoundVertices[1] = triangles[i + 1];
				triangleFoundVertices[2] = triangles[i + 2];
				
				triangleFound = true;
				break;	
			}
		}
		
		if (triangleFound) {
			vertexFound = true;
			
			//Console.WriteLine("vertexTriangle : " + uvBelongsToTriangle[0] + " " + uvBelongsToTriangle[1] + " " + uvBelongsToTriangle[2]);
			//Console.WriteLine("_UVs length: " + _UVs.Length);
			int vertex0 = triangleFoundVertices[0];
			int vertex1 = triangleFoundVertices[1];
			int vertex2 = triangleFoundVertices[2];
			
			Vector3 barycentricCoords = GetBarycentricCoords( _UVs[ vertex0 ], _UVs[ vertex1 ], _UVs[ vertex2 ], _uv);
			
			//Console.WriteLine("barycentricCoords: " + barycentricCoords);
			
			vertexPos = barycentricCoords.x * vertices[ vertex0 ] + barycentricCoords.y * vertices[ vertex1 ] + barycentricCoords.z * vertices[ vertex2 ];
		} else {
			//Console.WriteLine("no vertex found for uv " + _uv);
			vertexFound = false;
			vertexPos = Vector3.zero;
		}
		
		//if (triangleFound)
		//	Console.WriteLine("UV " + _uv.ToString("#.00000") + " belongs to triangle (" + vertexTriangle[0] + " " + vertexTriangle[1] + " " + vertexTriangle[2] + ")");
		//else
		//	Console.WriteLine("No triangle found for UV " + _uv.ToString("#.00000") );
	}
	
	
	public static bool IsPointWithinTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 p) {
		//0 <= s <= 1 and 0 <= t <= 1 and s + t <= 1
		float s = GetBarycentricX(v1, v2, v3, p);
		
		if ( !(s >= 0.0f && s <= 1.0f) )
			return false;
		
		float t = GetBarycentricY(v1, v2, v3, p);
		
		if ( !(t >= 0.0f && t <= 1.0f) )
			return false;
		
		if ( s + t <= 1.0f )
			return true;
		else
			return false;
	}
	
	public static float GetBarycentricX (Vector2 v1,Vector2 v2,Vector2 v3,Vector2 p)
	{
		return ((v2.y - v3.y)*(p.x-v3.x) + (v3.x - v2.x)*(p.y - v3.y)) /
			((v2.y-v3.y)*(v1.x-v3.x) + (v3.x-v2.x)*(v1.y -v3.y));
	}
	
	public static float GetBarycentricY (Vector2 v1,Vector2 v2,Vector2 v3,Vector2 p)
	{
		return ((v3.y - v1.y)*(p.x-v3.x) + (v1.x - v3.x)*(p.y - v3.y)) /
			((v3.y-v1.y)*(v2.x-v3.x) + (v1.x-v3.x)*(v2.y -v3.y));
	}
	
	public static Vector3 GetBarycentricCoords (Vector2 v1,Vector2 v2,Vector2 v3,Vector2 p)
	{
		Vector3 B = new Vector3();
		B.x = ((v2.y - v3.y)*(p.x-v3.x) + (v3.x - v2.x)*(p.y - v3.y)) /
			((v2.y-v3.y)*(v1.x-v3.x) + (v3.x-v2.x)*(v1.y -v3.y));
		B.y = ((v3.y - v1.y)*(p.x-v3.x) + (v1.x - v3.x)*(p.y - v3.y)) /
			((v3.y-v1.y)*(v2.x-v3.x) + (v1.x-v3.x)*(v2.y -v3.y));
		B.z = 1 - B.x - B.y;
		return B;
	}
	#endregion
	
	static public float SampleLayerHeight( Vector3 _position, int _layerMask ) {
		//We should only raycast downwards
		RaycastHit hitInfo;
		//LayerMask layerMask = 1 << terrainLayer;
		if ( Physics.Raycast(_position + 500.0f * Vector3.up, Vector3.down, out hitInfo, 1000.0f, _layerMask) ) {
			return hitInfo.point.y;
		}
		
		return 0.0f;
	}
	
	static public float SampleLayerSlope( Vector3 _position, int _layerMask ) {
		//We should only raycast downwards
		RaycastHit hitInfo;
		//LayerMask layerMask = 1 << terrainLayer;
		if ( Physics.Raycast(_position + 500.0f * Vector3.up, Vector3.down, out hitInfo, 1000.0f, _layerMask) ) {
			float cosSlope = Vector3.Dot( hitInfo.normal, Vector3.up );
			
			return Mathf.Rad2Deg * Mathf.Acos( cosSlope );
		}
		
		return 0.0f;
	}
	
	static public Vector3 SampleLayerNormal( Vector3 _position, int _layerMask ) {
		//We should only raycast downwards
		RaycastHit hitInfo;
		//LayerMask layerMask = 1 << terrainLayer;
		if ( Physics.Raycast(_position + 500.0f * Vector3.up, Vector3.down, out hitInfo, 1000.0f, _layerMask) ) {
			
			return hitInfo.normal;
		}
		
		return Vector3.up;
	}
	
	static public Texture2D SetPixelsForChannel(Texture2D _texture, float[] _pixels, WPColorChannels _channel) {
		Color[] srcPixels = _texture.GetPixels();
		
		for (int i = 0; i < _texture.width * _texture.height; i++) {
			switch (_channel) {
			default: case WPColorChannels.r:
				srcPixels[i].r = _pixels[i];
				break;
				
			case WPColorChannels.g:
				srcPixels[i].g = _pixels[i];
				break;
				
			case WPColorChannels.b:
				srcPixels[i].b = _pixels[i];
				break;
				
			case WPColorChannels.a:
				srcPixels[i].a = _pixels[i];
				break;
			}
		}
		
		Texture2D resultTexture = _texture;
		resultTexture.SetPixels( srcPixels );
		resultTexture.Apply();
		
		return resultTexture;
	}
	
	public enum TextureBlurMode {
		BlurAll = 0,
		BlurIgnoreAlphaPixelsOnly
	}
	
	static public Texture2D BlurTexture(Texture2D _texture, int blurSize) {
		return BlurTexture(_texture, blurSize, -1.0f, TextureBlurMode.BlurAll);
	}
	
	static public Texture2D BlurTexture(Texture2D _texture, int blurSize, float ignoreAlpha, TextureBlurMode _mode)
	{
		System.DateTime startTime = System.DateTime.Now;
		
		//Debug.Log("_texture.format: " + _texture.format.ToString() );
		
		Color[] origPixels = _texture.GetPixels();
		
		Color[] texPixels = _texture.GetPixels();
		
		bool shouldWriteAlpha = false;
		if (_texture.format == TextureFormat.ARGB32 || _texture.format == TextureFormat.RGBA32)
			shouldWriteAlpha = true;
		
	    // look at every pixel in the blur rectangle
	    for (int xx = 0; xx < _texture.width; xx++)
	    {
	        for (int yy = 0; yy < _texture.height; yy++)
	        {
				if (_mode == TextureBlurMode.BlurIgnoreAlphaPixelsOnly)
					if (origPixels[yy * _texture.width + xx].a != ignoreAlpha)
						continue;
				
	            float avgR = 0.0f, avgG = 0.0f, avgB = 0.0f, avgA = 0.0f;
	            int blurPixelCount = 0;
	 
	            // average the color of the red, green and blue for each pixel in the
	            // blur size while making sure you don't go outside the image bounds
	            for (int x = xx; (x < xx + blurSize && x < _texture.width); x++)
	            {
	                for (int y = yy; (y < yy + blurSize && y < _texture.height); y++)
	                {
	                    Color pixel = origPixels[y * _texture.width + x];
	 		
						//Ignore alpha is set
						if (ignoreAlpha >= 0.0f) {
							if (pixel.a != ignoreAlpha) {
			                    avgR += pixel.r;
			                    avgG += pixel.g;
			                    avgB += pixel.b;
								
								if (shouldWriteAlpha)
									avgA += pixel.a;
								
								blurPixelCount++;
							}
						} else {
							//Ignore alpha isn't set
							avgR += pixel.r;
		                    avgG += pixel.g;
		                    avgB += pixel.b;
							
							if (shouldWriteAlpha)
								avgA += pixel.a;
							
							blurPixelCount++;
						}
	                }
	            }
				
				//Debug.Log("blurPixelCount for (" + xx + "; " + yy + "): " + blurPixelCount);
	 
				if (blurPixelCount <= 0)
					continue;
				
	            avgR /= (float)blurPixelCount;
	            avgG /= (float)blurPixelCount;
	            avgB /= (float)blurPixelCount;
				avgA /= (float)blurPixelCount;
	 
	            // now that we know the average for the blur size, set each pixel to that color
	            for (int x = xx; x < xx + blurSize && x < _texture.width; x++) {
	                for (int y = yy; y < yy + blurSize && y < _texture.height; y++) {
						if (shouldWriteAlpha)
	                 	   texPixels[y * _texture.width + x] = new Color(avgR, avgG, avgB, avgA);
						else
							texPixels[y * _texture.width + x] = new Color(avgR, avgG, avgB);
					}
				}
	        }
	    }
		
		Texture2D resultTexture = new Texture2D(_texture.width, _texture.height, _texture.format, false);
		resultTexture.SetPixels( texPixels );
		resultTexture.Apply();
	 
		Debug.Log("Image blur took " + (System.DateTime.Now - startTime).TotalSeconds + " seconds.");
		
	    return resultTexture;
	}
	
	static private float GetGaussianWeight(float delta, float x, float y) {
		return ( 1.0f / (2.0f * Mathf.PI * delta * delta) ) * Mathf.Exp( -(x*x + y*y) / (2.0f * delta * delta) );
	}
	
	static private float GetExpandBlurWeight(float delta, float x, float y) {
		return 1.0f / delta * delta;
	}
	
	static private float GetBoxBlurWeight(float delta, float x, float y) {
		return 1.0f / ( (delta * 2 + 1) * (delta * 2 + 1) );
	}
	
	static private float GetGradientBlurWeight(float delta, float x, float y) {
		return ( 1.0f - Mathf.Sqrt(x*x + y*y) / delta) / ( (1.0f + delta) );
	}
	
	/*static public Texture2D Blur(Texture2D _texture, int blurSize, BlurType blurType, ColorChannels channels = ColorChannels.rgb)
	{
		System.DateTime startTime = System.DateTime.Now;
		
		Color[] srcPixels = _texture.GetPixels();
				
		switch (channels) {
			case ColorChannels.r:
				resPixels[y * _texture.width + x].r = blurredColor.r;
				break;
			
			case ColorChannels.g:
				resPixels[y * _texture.width + x].g = blurredColor.g;
				break;
			
			case ColorChannels.b:
				resPixels[y * _texture.width + x].b = blurredColor.b;
				break;
			
			default: case ColorChannels.rgb:
				resPixels[y * _texture.width + x].r = blurredColor.r;
				resPixels[y * _texture.width + x].g = blurredColor.g;
				resPixels[y * _texture.width + x].b = blurredColor.b;
				break;
		}
		
		Texture2D resultTexture = new Texture2D(_texture.width, _texture.height, _texture.format, false);
		resultTexture.SetPixels( resPixels );
		resultTexture.Apply();
	 
		Debug.Log("Image blur took " + (System.DateTime.Now - startTime).TotalSeconds + " seconds.");
		
	    return resultTexture;
	}*/
	
	static public WPGrayscaleImage Blur(WPGrayscaleImage _texture, int blurSize, WPBlurType blurType)
	{
		//System.DateTime startTime = System.DateTime.Now;
		
		//Debug.Log("blurSize: " + blurSize);
		
		//Build weight table
		float[,] blurWeightTable = new float[blurSize + 1,blurSize + 1];
		for (int x = 0; x <= blurSize; x++) {
			for (int y = 0; y <= blurSize; y++) {
				switch (blurType) {
					case WPBlurType.box:
						blurWeightTable[x,y] = GetBoxBlurWeight(blurSize, x, y);
						break;
					
					case WPBlurType.expand:
						blurWeightTable[x,y] = GetExpandBlurWeight(blurSize, x, y);
						break;
						
					default: case WPBlurType.gaussian:
						blurWeightTable[x,y] = GetGaussianWeight(blurSize, x, y);
						break;
				}
			}
			
		}
		
		byte[] srcPixels = _texture.GetPixels();
		byte[] resPixels = WPGrayscaleImage.ValuePixels(_texture.width, _texture.height, 0);
		
	    // look at every pixel in the blur rectangle
	    for (int x = 0; x < _texture.width; x++)
	    {
	        for (int y = 0; y < _texture.height; y++)
	        {
				//Keep alpha intact
				float blurredColor = 0.0f;
				
				//float g = 0.0f;
				
	            for (int xx = x - blurSize; xx <= x + blurSize; xx++) {
					if (xx < 0 || xx >= _texture.width)
						continue;
					
					for (int yy = y - blurSize; yy <= y + blurSize; yy++) {
						if (yy < 0 || yy >= _texture.height)
						continue;
						
						float blurWeight = blurWeightTable[ Mathf.Abs(xx-x), Mathf.Abs(yy-y)];
						
						//blurWeight = blurWeight * blurWeight;
						
						//float gaussianWeight = GetBoxBlurWeight(blurSize, x - xx, y - yy);
						
						//g += blurWeight * origPixels[yy * _texture.width + xx].g;
						
						blurredColor += blurWeight * (float)srcPixels[yy * _texture.width + xx] / 255.0f;
					}
				}
				
				//if (g > 1.0f)
				//	Debug.Log("g: " + g);
				
				//blurredColor.a = origPixels[y * _texture.width + x].a;
				
				resPixels[y * _texture.width + x] = (byte) (blurredColor * 255.0f);
	        }
	    }
		
		//GrayscaleImage resultTexture = new GrayscaleImage(_texture.width, _texture.height, resPixels);
	 
		//Debug.Log("Image blur took " + (System.DateTime.Now - startTime).TotalSeconds + " seconds.");
		
	    return new WPGrayscaleImage(_texture.width, _texture.height, resPixels);
	}
	
	public static Color[] CopyPixels(Texture2D _texture) {
		Color[] srcPixels = _texture.GetPixels();
		Color[] dstPixels = new Color[_texture.width * _texture.height];
		
		for (int i = 0; i < _texture.width * _texture.height; i++) {
			dstPixels[i].r = srcPixels[i].r;
			dstPixels[i].g = srcPixels[i].g;
			dstPixels[i].b = srcPixels[i].b;
			dstPixels[i].a = srcPixels[i].a;
		}
		
		return dstPixels;
	}
	
	public static Texture2D CopyTexture(Texture2D _texture) {		
		Texture2D resultTexture = new Texture2D(_texture.width, _texture.height, _texture.format, false);
		resultTexture.SetPixels( CopyPixels(_texture) );
		resultTexture.Apply();
		
		return resultTexture;
	}
	
	/*static public Texture2D GradientTest(Texture2D _borderMask, Texture2D _gradientMask, int gradientSize, GradientType gradientType,
								ColorChannels borderChannel, ColorChannels gradientChannel) {
		if (_borderMask.width != _gradientMask.width || _borderMask.height != _gradientMask.height) {
			Debug.LogError("Cannot apply gradient - dimensions mismatch.");
			return null;
		}
		
		Color[] gradientMaskPixels = _gradientMask.GetPixels();
		Color[] borderMaskPixels = _borderMask.GetPixels();
		Color[] resPixels = _borderMask.GetPixels();
		
		//Optimization
		//Make a list of border pixels
		List<Vector2> borderPixels = new List<Vector2>();
		
		for (int x = 0; x < _borderMask.width; x++) {
			for (int y = 0; y < _borderMask.height; y++) {
				switch (borderChannel) {
				case ColorChannels.r:
					if ( borderMaskPixels[y * _borderMask.width + x].r >= 1.0f ) {
						Vector2 tempPixel = new Vector2(x, y);
						borderPixels.Add(tempPixel);
					}
					break;
					
				case ColorChannels.g:
					if ( borderMaskPixels[y * _borderMask.width + x].g >= 1.0f ) {
						Vector2 tempPixel = new Vector2(x, y);
						borderPixels.Add(tempPixel);
					}
					break;
					
				case ColorChannels.b:
					if ( borderMaskPixels[y * _borderMask.width + x].b >= 1.0f ) {
						Vector2 tempPixel = new Vector2(x, y);
						borderPixels.Add(tempPixel);
					}
					break;
					
				default: case ColorChannels.a:
					if ( borderMaskPixels[y * _borderMask.width + x].a >= 1.0f ) {
						Vector2 tempPixel = new Vector2(x, y);
						borderPixels.Add(tempPixel);
					}
					break;
				}
			}
			
		}
		
		Debug.Log("border pixels: " + borderPixels.Count);
		return _gradientMask;
		
		for (int x = 0; x < _gradientMask.width; x++) {
			for (int y = 0; y < _gradientMask.height; y++) {
				
				//
				//Find closest border pixel
				float closestSqrDistance = 10000000.0f;
				Vector2 closestBorderPixel = Vector2.zero;
				
				Vector2 currentPixel = new Vector2(x, y);
				
				foreach (Vector2 borderPixel in borderPixels) {
					float sqrMagnitude = (borderPixel - currentPixel).sqrMagnitude;
					if ( sqrMagnitude < closestSqrDistance) {
						closestSqrDistance = sqrMagnitude;
						closestBorderPixel = borderPixel;
						
						if (sqrMagnitude <= 0.0f)
							break;
					}
				}
				
				float distanceToBorder = (currentPixel - closestBorderPixel).magnitude;
				
				if (distanceToBorder <= gradientSize) {
					float gradientAmount = distanceToBorder / gradientSize;
					
					switch (gradientType) {
					default: case GradientType.linear:
						gradientAmount = 1.0f - gradientAmount;
						break;
						
					case GradientType.oneMinusSqr:
						gradientAmount = 1.0f - gradientAmount * gradientAmount;
						break;
						
					case GradientType.sqrOfOneMinusG:
						gradientAmount = 1.0f - gradientAmount;
						gradientAmount = gradientAmount * gradientAmount;
						break;
					}
					
					//gradientAmount = gradientAmount * gradientAmount;
					
					switch (gradientChannel) {
					case ColorChannels.r:
						resPixels[y * _gradientMask.width + x].r = gradientAmount;
						break;
						
					case ColorChannels.g:
						resPixels[y * _gradientMask.width + x].g = gradientAmount;
						break;
						
					case ColorChannels.b:
						resPixels[y * _gradientMask.width + x].b = gradientAmount;
						break;
						
					default: case ColorChannels.a:
						resPixels[y * _gradientMask.width + x].a = gradientAmount;
						break;
					}
				 	
				}
					
			}
		}
		
		Texture2D resTexture = _gradientMask;
	
		//Debug.Log("gradientPixels: " + gradientPixels);
		resTexture.SetPixels( resPixels );
		resTexture.Apply();
		
		return resTexture;
	}*/
	
	/*static public Texture2D Gradient(Texture2D _borderMask, Texture2D _gradientMask, int gradientSize, GradientType gradientType,
								ColorChannels borderChannel, ColorChannels gradientChannel) {
		GPGPUComputations tempGPGPU = new GPGPUComputations();
		
		
		bool isGPGPUAvailable = tempGPGPU.Setup();
		
		if (!isGPGPUAvailable) {
			Debug.LogWarning("GPGPU isn't available. Please install a GPGPU compatible video card driver for best results.");
			if (proceedIfGPGPUFails)
				return GradientCPU(_borderMask, _gradientMask, gradientSize, gradientType, borderChannel, gradientChannel);
			else
				return null;
		}
		
		//Save the temp image for GPGPU
		string bmTempFilePath = Application.dataPath + "/" + "BMTemp.png";
		string gmTempFilePath = Application.dataPath + "/" + "GMTemp.png";
		string outputTempFilePath = Application.dataPath + "/" + "GPGPUOutput.png";
		
		SaveTextureAsPng(_borderMask, bmTempFilePath);
		SaveTextureAsPng(_gradientMask, gmTempFilePath);
		
		
		bool gpgpuGradientResult = tempGPGPU.ApplyGradient(bmTempFilePath, gmTempFilePath, outputTempFilePath,
					10, gradientSize, (int)borderChannel, (int)gradientChannel);
		
		if (!gpgpuGradientResult)  {
			Debug.LogError("GPGPU gradient: failed.");
			return null;
		}
		
		if ( !File.Exists(outputTempFilePath) ) {
			Debug.LogError("GPGPU gradient: output file wasn't created.");
			return null;
		}
		
		Texture2D resultTexture = AssetDatabase.LoadAssetAtPath("Assets/GPGPUOutput.png", typeof(Texture2D) ) as Texture2D;
		
		File.Delete(bmTempFilePath);
		File.Delete(gmTempFilePath);
		//File.Delete(outputTempFilePath);
		
		
		return resultTexture;
		//return _borderMask;
	}*/
	
	static public WPGrayscaleImage ExpandBorder(WPGrayscaleImage _borderMask, int expandByPixels) {
		byte[] borderMaskPixels = _borderMask.GetPixels();
		byte[] resPixels = WPGrayscaleImage.ValuePixels(_borderMask.width, _borderMask.height, 0);	//Fill with black
		
		//Vector2 geometricalCenter = CalculateGeometricalCenter(_borderMask, 1.0f, .01f);
		
		//Debug.Log("");
		
		//int totalSearchIterations = 0;
		for (int x = 0; x < _borderMask.width; x++) {
			for (int y = 0; y < _borderMask.height; y++) {
				//Keep only border pixels
				if ( borderMaskPixels[y * _borderMask.width + x] < 255)
					continue;
				
				for (int xx = x - expandByPixels; xx <= x + expandByPixels; xx++) {
					if (xx < 0 || xx >= _borderMask.width)
						continue;
					
					for (int yy = y - expandByPixels; yy <= y + expandByPixels; yy++) {
						if (yy < 0 || yy >= _borderMask.height)
							continue;

						resPixels[yy * _borderMask.width + xx] = 255;
					}
				}
					
			}
		}
		
		WPGrayscaleImage resTexture = new WPGrayscaleImage(_borderMask.width, _borderMask.height, resPixels);
		
		return resTexture;
	}
	
	//Non-masked gradient
	/*static public Texture2D Gradient(Texture2D _borderMask, int gradientSize, float gradientMin, float gradientMax,
								GradientType gradientType,
								ColorChannels borderChannel, ColorChannels gradientChannel) {
		System.DateTime startTime = System.DateTime.Now;
		
		Debug.Log("Helper.Gradient");
		
		Color[] borderMaskPixels = _borderMask.GetPixels();
		Color[] resPixels = _borderMask.GetPixels();
		
		Vector2 geometricalCenter = CalculateGeometricalCenter(_borderMask, Color.white, .01f, borderChannel);
		
		//Debug.Log("");
		
		//int totalSearchIterations = 0;
		for (int x = 0; x < _borderMask.width; x++) {
			for (int y = 0; y < _borderMask.height; y++) {
				//Gradient itself
				
				//Try fast version first. If it fails, go with the circle search.
				Vector2 closestPixelCoords = Vector2.zero;
				bool closestPixelFound = false;
				
				Vector2 startPixel = new Vector2( (float)x, (float)y );
				Vector2 directionToCenter = geometricalCenter - startPixel;
				directionToCenter.Normalize();
				
				//Consider the case when we're already at the center
				if (directionToCenter.sqrMagnitude > 1.0f) {
					for (float r = 0.0f; !closestPixelFound; r += .9f) {
						//if (x == _borderMask.width / 2)
						//Debug.Log("r: " + r);
						
						Vector2 lookupPixel = startPixel + directionToCenter * r;
						
						int xx = (int)lookupPixel.x;
						int yy = (int)lookupPixel.y;
						
						if (xx < 0 || xx >= _borderMask.width || yy < 0 || yy >= _borderMask.height)
							break;
						
						//Reached the border?
						switch (borderChannel) {
						case ColorChannels.r:
							if ( borderMaskPixels[yy * _borderMask.width + xx].r > 0.0f ) {
								closestPixelFound = true;
							}
							break;
							
						case ColorChannels.g:
							if ( borderMaskPixels[yy * _borderMask.width + xx].g > 0.0f ) {
								closestPixelFound = true;
							}
							break;
							
						case ColorChannels.b:
							if ( borderMaskPixels[yy * _borderMask.width + xx].b > 0.0f ) {
								closestPixelFound = true;
							}
							break;
							
						default: case ColorChannels.a:
							if ( borderMaskPixels[yy * _borderMask.width + xx].a > 0.0f ) {
								closestPixelFound = true;
							}
							break;
						}
						
						//If didn't reach the border, check surrounding pixels
						
						if (!closestPixelFound) {
							for (int xxx = xx - 1; xxx <= xx + 1 && !closestPixelFound; xxx++) {
								for (int yyy = yy - 1; yyy <= yy + 1; yyy++) {
									if (xxx < 0 || xxx >= _borderMask.width || yyy < 0 || yyy >= _borderMask.height)
										continue;
									
									switch (borderChannel) {
									case ColorChannels.r:
										if ( borderMaskPixels[yyy * _borderMask.width + xxx].r > 0.0f ) {
											closestPixelFound = true;
										}
										break;
										
									case ColorChannels.g:
										if ( borderMaskPixels[yyy * _borderMask.width + xxx].g > 0.0f ) {
											closestPixelFound = true;
										}
										break;
										
									case ColorChannels.b:
										if ( borderMaskPixels[yyy * _borderMask.width + xxx].b > 0.0f ) {
											closestPixelFound = true;
										}
										break;
										
									default: case ColorChannels.a:
										if ( borderMaskPixels[yyy * _borderMask.width + xxx].a > 0.0f ) {
											closestPixelFound = true;
										}
										break;
									}
									
									if (closestPixelFound) {
										closestPixelCoords = new Vector2(xxx, yyy);
										break;
									}
								}
								//if (closestPixelFound)
								//	break;
							}
							
						} else {
							closestPixelCoords = new Vector2(xx, yy);
							break;
						}
					}	
				}
				
				//if (x == _borderMask.width / 2)
					//Debug.Log("closestPixelFound: " + closestPixelFound + " closestPixelCoords: " + closestPixelCoords.ToString() );
				
				float radius = 0.0f;
				while (!closestPixelFound) {
					//totalSearchIterations++;
					float deltaAngle = 1.0f / radius;
					
					for (float angle = 0.0f; angle <= 2.0f * Mathf.PI; angle += deltaAngle) {
						int xx = x + (int) ( radius * Mathf.Cos(angle) );
						int yy = y + (int) ( radius * Mathf.Sin(angle) );
						
						//Are we within texture bounds?
						if (xx < 0 || yy < 0 || xx >= _borderMask.width || yy >= _borderMask.height) {
							//closestPixelFound = true;
							//closestPixelCoords = new Vector2(x, y);
							continue;
						}
					
						switch (borderChannel) {
						case ColorChannels.r:
							if ( borderMaskPixels[yy * _borderMask.width + xx].r > 0.0f ) {
								closestPixelFound = true;
							}
							break;
							
						case ColorChannels.g:
							if ( borderMaskPixels[yy * _borderMask.width + xx].g > 0.0f ) {
								closestPixelFound = true;
							}
							break;
							
						case ColorChannels.b:
							if ( borderMaskPixels[yy * _borderMask.width + xx].b > 0.0f ) {
								closestPixelFound = true;
							}
							break;
							
						default: case ColorChannels.a:
							if ( borderMaskPixels[yy * _borderMask.width + xx].a > 0.0f ) {
								closestPixelFound = true;
							}
							break;
						}
					
						if (closestPixelFound) {
							closestPixelCoords = new Vector2(xx, yy);
							break;
						}
					}
					
					//Out of bounds
					if (radius > _borderMask.width * 1.42f && radius > _borderMask.height * 1.42f) {
						//Debug.Log("search pixel is out of bounds.");
						break;
					}
						
					radius += 1.0f;
				}
				
				if (!closestPixelFound) {
					Debug.LogError("no closest pixel found!!!");
					break;
				}
				
				float distanceToBorder = ( (new Vector2(x, y) ) - closestPixelCoords).magnitude;
				
				//if (distanceToBorder <= gradientSize)
				{
					float gradientAmount = distanceToBorder / gradientSize;
					
					switch (gradientType) {
					default: case GradientType.linear:
						gradientAmount = 1.0f - gradientAmount;
						break;
						
					case GradientType.oneMinusSqr:
						gradientAmount = 1.0f - gradientAmount * gradientAmount;
						break;
						
					case GradientType.sqrOfOneMinusG:
						gradientAmount = 1.0f - gradientAmount;
						gradientAmount = gradientAmount * gradientAmount;
						break;
					}
					
					//Gradient from border's intensity
//					switch (borderChannel) {
//					case ColorChannels.r:
//						gradientAmount = gradientAmount * borderMaskPixels[ (int) (closestPixelCoords.y) * _borderMask.width + (int) (closestPixelCoords.x)].r;
//						break;
//						
//					case ColorChannels.g:
//						gradientAmount = gradientAmount * borderMaskPixels[ (int) (closestPixelCoords.y) * _borderMask.width + (int) (closestPixelCoords.x)].g;
//						break;
//						
//					case ColorChannels.b:
//						gradientAmount = gradientAmount * borderMaskPixels[ (int) (closestPixelCoords.y) * _borderMask.width + (int) (closestPixelCoords.x)].b;
//						break;
//						
//					default: case ColorChannels.a:
//						gradientAmount = gradientAmount * borderMaskPixels[ (int) (closestPixelCoords.y) * _borderMask.width + (int) (closestPixelCoords.x)].a;
//						break;
//					}
					
					gradientAmount = gradientAmount * (gradientMax - gradientMin) + gradientMin;
					
					//Apply the gradient
					switch (gradientChannel) {
					case ColorChannels.r:
						resPixels[y * _borderMask.width + x].r = gradientAmount;
						break;
						
					case ColorChannels.g:
						resPixels[y * _borderMask.width + x].g = gradientAmount;
						break;
						
					case ColorChannels.b:
						resPixels[y * _borderMask.width + x].b = gradientAmount;
						break;
						
					default: case ColorChannels.a:
						resPixels[y * _borderMask.width + x].a = gradientAmount;
						break;
					}
				 	
				}
					
			}
		}
		
		//Debug.Log("totalSearchIterations: " + totalSearchIterations);
		
		Texture2D resTexture = _borderMask;//new Texture2D(_borderMask.width, _borderMask.height, _borderMask.format, false);
	
		//Debug.Log("gradientPixels: " + gradientPixels);
		resTexture.SetPixels( resPixels );
		resTexture.Apply();
		
		Debug.Log("Gradient took " + (System.DateTime.Now - startTime).TotalSeconds + " seconds.");
		
		return resTexture;
	}
	
	//Masked gradient
	static public Texture2D Gradient(Texture2D _borderMask, Texture2D _gradientMask, int gradientSize, GradientType gradientType,
								ColorChannels borderChannel, ColorChannels gradientChannel) {
		if (_borderMask.width != _gradientMask.width || _borderMask.height != _gradientMask.height) {
			Debug.LogError("Cannot apply gradient - dimensions mismatch.");
			return null;
		}
		
		Color[] gradientMaskPixels = _gradientMask.GetPixels();
		Color[] borderMaskPixels = _borderMask.GetPixels();
		Color[] resPixels = _borderMask.GetPixels();
		
		Vector2 geometricalCenter = CalculateGeometricalCenter(_borderMask, Color.white, .01f, borderChannel);
		
		//int totalSearchIterations = 0;
		for (int x = 0; x < _borderMask.width; x++) {
			for (int y = 0; y < _borderMask.height; y++) {
				//Skip non gradient mask pixels
				switch (borderChannel) {
				case ColorChannels.r:
					if ( gradientMaskPixels[y * _gradientMask.width + x].r <= 0.0f ) {
						continue;
					}
					break;
					
				case ColorChannels.g:
					if ( gradientMaskPixels[y * _gradientMask.width + x].g <= 0.0f ) {
						continue;
					}
					break;
					
				case ColorChannels.b:
					if ( gradientMaskPixels[y * _gradientMask.width + x].b <= 0.0f ) {
						continue;
					}
					break;
					
				default: case ColorChannels.a:
					if ( gradientMaskPixels[y * _gradientMask.width + x].a <= 0.0f ) {
						continue;
					}
					break;
				}
			
				//Gradient itself
				Vector2 closestPixelCoords = Vector2.zero;
				bool closestPixelFound = false;
				
				Vector2 startPixel = new Vector2( (float)x, (float)y );
				Vector2 directionToCenter = geometricalCenter - startPixel;
				directionToCenter.Normalize();
				
				//Consider the case when we're already at the center
				if (directionToCenter.sqrMagnitude > 1.0f) {
					for (float r = 0.0f; !closestPixelFound; r += .9f) {
						//if (x == _borderMask.width / 2)
						//Debug.Log("r: " + r);
						
						Vector2 lookupPixel = startPixel + directionToCenter * r;
						
						int xx = (int)lookupPixel.x;
						int yy = (int)lookupPixel.y;
						
						if (xx < 0 || xx >= _borderMask.width || yy < 0 || yy >= _borderMask.height)
							break;
						
						//Reached the border?
						switch (borderChannel) {
						case ColorChannels.r:
							if ( borderMaskPixels[yy * _borderMask.width + xx].r > 0.0f ) {
								closestPixelFound = true;
							}
							break;
							
						case ColorChannels.g:
							if ( borderMaskPixels[yy * _borderMask.width + xx].g > 0.0f ) {
								closestPixelFound = true;
							}
							break;
							
						case ColorChannels.b:
							if ( borderMaskPixels[yy * _borderMask.width + xx].b > 0.0f ) {
								closestPixelFound = true;
							}
							break;
							
						default: case ColorChannels.a:
							if ( borderMaskPixels[yy * _borderMask.width + xx].a > 0.0f ) {
								closestPixelFound = true;
							}
							break;
						}
						
						//If didn't reach the border, check surrounding pixels
						
						if (!closestPixelFound) {
							for (int xxx = xx - 1; xxx <= xx + 1 && !closestPixelFound; xxx++) {
								for (int yyy = yy - 1; yyy <= yy + 1; yyy++) {
									if (xxx < 0 || xxx >= _borderMask.width || yyy < 0 || yyy >= _borderMask.height)
										continue;
									
									switch (borderChannel) {
									case ColorChannels.r:
										if ( borderMaskPixels[yyy * _borderMask.width + xxx].r > 0.0f ) {
											closestPixelFound = true;
										}
										break;
										
									case ColorChannels.g:
										if ( borderMaskPixels[yyy * _borderMask.width + xxx].g > 0.0f ) {
											closestPixelFound = true;
										}
										break;
										
									case ColorChannels.b:
										if ( borderMaskPixels[yyy * _borderMask.width + xxx].b > 0.0f ) {
											closestPixelFound = true;
										}
										break;
										
									default: case ColorChannels.a:
										if ( borderMaskPixels[yyy * _borderMask.width + xxx].a > 0.0f ) {
											closestPixelFound = true;
										}
										break;
									}
									
									if (closestPixelFound) {
										closestPixelCoords = new Vector2(xxx, yyy);
										break;
									}
								}
								//if (closestPixelFound)
								//	break;
							}
							
						} else {
							closestPixelCoords = new Vector2(xx, yy);
							break;
						}
					}	
				}
				
				//if (x == _borderMask.width / 2)
					//Debug.Log("closestPixelFound: " + closestPixelFound + " closestPixelCoords: " + closestPixelCoords.ToString() );
				
				float radius = 0.0f;
				while (!closestPixelFound) {
					//totalSearchIterations++;
					float deltaAngle = 1.0f / radius;
					
					for (float angle = 0.0f; angle <= 2.0f * Mathf.PI; angle += deltaAngle) {
						int xx = x + (int) ( radius * Mathf.Cos(angle) );
						int yy = y + (int) ( radius * Mathf.Sin(angle) );
						
						//Are we within texture bounds?
						if (xx < 0 || yy < 0 || xx >= _borderMask.width || yy >= _borderMask.height) {
							continue;
						}
					
					switch (borderChannel) {
					case ColorChannels.r:
						if ( borderMaskPixels[yy * _borderMask.width + xx].r >= 1.0f ) {
							closestPixelFound = true;
						}
						break;
						
					case ColorChannels.g:
						if ( borderMaskPixels[yy * _borderMask.width + xx].g >= 1.0f ) {
							closestPixelFound = true;
						}
						break;
						
					case ColorChannels.b:
						if ( borderMaskPixels[yy * _borderMask.width + xx].b >= 1.0f ) {
							closestPixelFound = true;
						}
						break;
						
					default: case ColorChannels.a:
						if ( borderMaskPixels[yy * _borderMask.width + xx].a >= 1.0f ) {
							closestPixelFound = true;
						}
						break;
					}
					
						if (closestPixelFound) {
							closestPixelCoords = new Vector2(xx, yy);
							break;
						}
					}
					
					if (closestPixelFound)
						break;
					
					//Out of bounds
					if (radius > _borderMask.width * 1.42f && radius > _borderMask.height * 1.42f) {
						//Debug.Log("search pixel is out of bounds.");
						break;
					}
						
					radius += 1.0f;
				}
				
				if (!closestPixelFound) {
					Debug.LogError("no closest pixel found.");
					continue;
				}
				
				float distanceToBorder = ( (new Vector2(x, y) ) - closestPixelCoords).magnitude;
				
				if (distanceToBorder <= gradientSize) {
					float gradientAmount = distanceToBorder / gradientSize;
					
					switch (gradientType) {
					default: case GradientType.linear:
						gradientAmount = 1.0f - gradientAmount;
						break;
						
					case GradientType.oneMinusSqr:
						gradientAmount = 1.0f - gradientAmount * gradientAmount;
						break;
						
					case GradientType.sqrOfOneMinusG:
						gradientAmount = 1.0f - gradientAmount;
						gradientAmount = gradientAmount * gradientAmount;
						break;
					}
					
					//gradientAmount = gradientAmount * gradientAmount;
					
					switch (gradientChannel) {
					case ColorChannels.r:
						resPixels[y * _borderMask.width + x].r = gradientAmount;
						break;
						
					case ColorChannels.g:
						resPixels[y * _borderMask.width + x].g = gradientAmount;
						break;
						
					case ColorChannels.b:
						resPixels[y * _borderMask.width + x].b = gradientAmount;
						break;
						
					default: case ColorChannels.a:
						resPixels[y * _borderMask.width + x].a = gradientAmount;
						break;
					}
				 	
				}
					
			}
		}
		
		//Debug.Log("totalSearchIterations: " + totalSearchIterations);
		
		Texture2D resTexture = new Texture2D(_borderMask.width, _borderMask.height, _borderMask.format, false);
	
		//Debug.Log("gradientPixels: " + gradientPixels);
		resTexture.SetPixels( resPixels );
		resTexture.Apply();
		
		//return _borderMask;
		
		return resTexture;
	}*/
	
		static public WPGrayscaleImage Gradient(WPGrayscaleImage _borderMask, int gradientSize, float gradientMin, float gradientMax,
								WPGradientType gradientType) {
		//System.DateTime startTime = System.DateTime.Now;
		
		//Debug.Log("Gradient input image width: " + _borderMask.width);
		
		//A faster version of gradient by using consequtive dilations on the border
		/*GrayscaleImage[] dilatedBorders = new GrayscaleImage[gradientSize + 1];
		
		dilatedBorders[0] = _borderMask;
		
		for (int i = 1; i <= gradientSize; i++) {
			//Expanding the previous border by only 1 will be much faster
			dilatedBorders[i] = ExpandBorder(dilatedBorders[i - 1], 1);
		}*/
		
		byte[] resPixels = WPGrayscaleImage.ValuePixels(_borderMask.width, _borderMask.height, 255);
		/*byte[] timesAdded = new byte[_borderMask.width * _borderMask.height];	//For interpolation
		
		for (int i = 0; i < _borderMask.width * _borderMask.height; i++) {
			timesAdded[i] = 0;	
		}*/
		
		//Colorize (start from the end, i.e. most expanded borders).
		for (int i = gradientSize; i >= 0; i--) {
			WPGrayscaleImage dilatedBorder = ExpandBorder(_borderMask, i);
			byte[] currentBorderPixels = dilatedBorder.GetPixels();
			for (int x = 0; x < _borderMask.width; x++) {
				for (int y = 0; y < _borderMask.height; y++) {
					if ( currentBorderPixels[y * _borderMask.width + x] < 255 )
						continue;
	
					float gradientAmount = (float)i / (float)gradientSize;
					
					switch (gradientType) {
					default: case WPGradientType.linear:
						gradientAmount = 1.0f - gradientAmount;
						break;
						
					case WPGradientType.oneMinusSqr:
						gradientAmount = 1.0f - gradientAmount * gradientAmount;
						break;
						
					case WPGradientType.sqrOfOneMinusG:
						gradientAmount = 1.0f - gradientAmount;
						gradientAmount = gradientAmount * gradientAmount;
						break;
					}
					
					gradientAmount = gradientAmount * (gradientMax - gradientMin) + gradientMin;
					//resPixels[y * _borderMask.width + x] += (int) (gradientAmount * 255.0f);
					//timesAdded[y * _borderMask.width + x]++;
					resPixels[y * _borderMask.width + x] = (byte) (gradientAmount * 255.0f);
				}
			}
			
			System.GC.Collect();
		}
		
		//Interpolate
		/*for (int i = 0; i < _borderMask.width * _borderMask.height; i++) {
			if (timesAdded[i] <= 0)
				continue;
			
			resPixels[i] = (int) ( (float)resPixels[i] / (float)timesAdded[i] );
		}*/
		
		WPGrayscaleImage resTexture = new WPGrayscaleImage(_borderMask.width, _borderMask.height, resPixels);
		
		//Debug.Log("Gradient took " + (System.DateTime.Now - startTime).TotalSeconds + " seconds.");
		
		return resTexture;
	}
	
	//Non-masked gradient
//	static public GrayscaleImage GradientSlow(GrayscaleImage _borderMask, int gradientSize, float gradientMin, float gradientMax,
//								GradientType gradientType) {
//		System.DateTime startTime = System.DateTime.Now;
//		
//		//Debug.Log("Helper.Gradient");
//		
//		float[] borderMaskPixels = _borderMask.GetPixels();
//		float[] resPixels = GrayscaleImage.ValuePixels(_borderMask.width, _borderMask.height, 0.0f);
//		
//		Vector2 geometricalCenter = CalculateGeometricalCenter(_borderMask, 1.0f, .01f);
//		
//		//Debug.Log("");
//		
//		//int totalSearchIterations = 0;
//		for (int x = 0; x < _borderMask.width; x++) {
//			for (int y = 0; y < _borderMask.height; y++) {
//				//Gradient itself
//				
//				//Try fast version first. If it fails, go with the circle search.
//				Vector2 closestPixelCoords = Vector2.zero;
//				bool closestPixelFound = false;
//				
//				/*Vector2 startPixel = new Vector2( (float)x, (float)y );
//				Vector2 directionToCenter = geometricalCenter - startPixel;
//				directionToCenter.Normalize();
//				
//				//Consider the case when we're already at the center
//				if (directionToCenter.sqrMagnitude > 1.0f) {
//					for (float r = 0.0f; !closestPixelFound; r += .9f) {
//						//if (x == _borderMask.width / 2)
//						//Debug.Log("r: " + r);
//						
//						Vector2 lookupPixel = startPixel + directionToCenter * r;
//						
//						int xx = (int)lookupPixel.x;
//						int yy = (int)lookupPixel.y;
//						
//						if (xx < 0 || xx >= _borderMask.width || yy < 0 || yy >= _borderMask.height)
//							break;
//						
//						//Reached the border?
//						if ( borderMaskPixels[yy * _borderMask.width + xx] > 0.0f )
//							closestPixelFound = true;
//						
//						//If didn't reach the border, check surrounding pixels
//						
//						if (!closestPixelFound) {
//							for (int xxx = xx - 1; xxx <= xx + 1 && !closestPixelFound; xxx++) {
//								for (int yyy = yy - 1; yyy <= yy + 1; yyy++) {
//									if (xxx < 0 || xxx >= _borderMask.width || yyy < 0 || yyy >= _borderMask.height)
//										continue;
//									
//									if ( borderMaskPixels[yyy * _borderMask.width + xxx] > 0.0f )
//										closestPixelFound = true;
//									
//									if (closestPixelFound) {
//										closestPixelCoords = new Vector2(xxx, yyy);
//										break;
//									}
//								}
//								//if (closestPixelFound)
//								//	break;
//							}
//							
//						} else {
//							closestPixelCoords = new Vector2(xx, yy);
//							break;
//						}
//					}	
//				}*/
//				
//				//if (x == _borderMask.width / 2)
//					//Debug.Log("closestPixelFound: " + closestPixelFound + " closestPixelCoords: " + closestPixelCoords.ToString() );
//				
//				float radius = 0.0f;
//				while (!closestPixelFound) {
//					//totalSearchIterations++;
//					float deltaAngle = 1.0f / radius;
//					
//					for (float angle = 0.0f; angle <= 2.0f * Mathf.PI; angle += deltaAngle) {
//						int xx = x + (int) ( radius * Mathf.Cos(angle) );
//						int yy = y + (int) ( radius * Mathf.Sin(angle) );
//						
//						//Are we within texture bounds?
//						if (xx < 0 || yy < 0 || xx >= _borderMask.width || yy >= _borderMask.height) {
//							//closestPixelFound = true;
//							//closestPixelCoords = new Vector2(x, y);
//							continue;
//						}
//					
//						if ( borderMaskPixels[yy * _borderMask.width + xx] > 0.0f )
//							closestPixelFound = true;
//					
//						if (closestPixelFound) {
//							closestPixelCoords = new Vector2(xx, yy);
//							break;
//						}
//					}
//					
//					/*if (closestPixelFound)
//						break;*/
//					
//					//Out of bounds
//					if (radius > _borderMask.width * 1.42f && radius > _borderMask.height * 1.42f) {
//						//Debug.Log("search pixel is out of bounds.");
//						break;
//					}
//						
//					radius += 1.0f;
//				}
//				
//				if (!closestPixelFound) {
//					Debug.LogError("no closest pixel found!!!");
//					break;
//				}
//				
//				float distanceToBorder = ( (new Vector2(x, y) ) - closestPixelCoords).magnitude;
//				
//				//if (distanceToBorder <= gradientSize)
//				{
//					float gradientAmount = distanceToBorder / gradientSize;
//					
//					switch (gradientType) {
//					default: case GradientType.linear:
//						gradientAmount = 1.0f - gradientAmount;
//						break;
//						
//					case GradientType.oneMinusSqr:
//						gradientAmount = 1.0f - gradientAmount * gradientAmount;
//						break;
//						
//					case GradientType.sqrOfOneMinusG:
//						gradientAmount = 1.0f - gradientAmount;
//						gradientAmount = gradientAmount * gradientAmount;
//						break;
//					}
//					
//					//Gradient from border's intensity
//					gradientAmount = gradientAmount * (gradientMax - gradientMin) + gradientMin;
//					
//					//Apply the gradient
//					resPixels[y * _borderMask.width + x] = gradientAmount;
//				}
//					
//			}
//		}
//		
//		//Debug.Log("totalSearchIterations: " + totalSearchIterations);
//		
//		GrayscaleImage resTexture = new GrayscaleImage(_borderMask.width, _borderMask.height, resPixels);
//		
//		Debug.Log("Gradient took " + (System.DateTime.Now - startTime).TotalSeconds + " seconds.");
//		
//		return resTexture;
//	}
//	
//	//Masked gradient
//	static public GrayscaleImage Gradient(GrayscaleImage _borderMask, GrayscaleImage _gradientMask, int gradientSize, GradientType gradientType) {
//		if (_borderMask.width != _gradientMask.width || _borderMask.height != _gradientMask.height) {
//			Debug.LogError("Cannot apply gradient - dimensions mismatch.");
//			return null;
//		}
//		
//		//return _gradientMask;
//		
//		float[] gradientMaskPixels = _gradientMask.GetPixels();
//		float[] borderMaskPixels = _borderMask.GetPixels();
//		float[] resPixels = GrayscaleImage.ValuePixels(_borderMask.width, _borderMask.height, 0.0f);
//		
//		Vector2 geometricalCenter = CalculateGeometricalCenter(_borderMask, 1.0f, .1f);
//		
//		//return _borderMask;
//		
//		//int totalSearchIterations = 0;
//		for (int x = 0; x < _borderMask.width; x++) {
//			for (int y = 0; y < _borderMask.height; y++) {
//				//Skip non gradient mask pixels
//				if ( gradientMaskPixels[y * _gradientMask.width + x] <= 0.0f )
//					continue;
//			
//				//Gradient itself
//				Vector2 closestPixelCoords = Vector2.zero;
//				bool closestPixelFound = false;
//				
//				/*Vector2 startPixel = new Vector2( (float)x, (float)y );
//				Vector2 directionToCenter = geometricalCenter - startPixel;
//				directionToCenter.Normalize();
//				
//				//Consider the case when we're already at the center
//				if (directionToCenter.sqrMagnitude > 1.0f) {
//					for (float r = 0.0f; !closestPixelFound; r += .5f) {
//						//if (x == _borderMask.width / 2)
//						//Debug.Log("r: " + r);
//						
//						Vector2 lookupPixel = startPixel + directionToCenter * r;
//						
//						int xx = (int)lookupPixel.x;
//						int yy = (int)lookupPixel.y;
//						
//						if (xx < 0 || xx >= _borderMask.width || yy < 0 || yy >= _borderMask.height)
//							break;
//						
//						//Reached the border?
//						if ( borderMaskPixels[yy * _borderMask.width + xx] > 0.0f )
//								closestPixelFound = true;
//						
//						//If didn't reach the border, check surrounding pixels
//						
//						if (!closestPixelFound) {
//							for (int xxx = xx - 1; xxx <= xx + 1 && !closestPixelFound; xxx++) {
//								for (int yyy = yy - 1; yyy <= yy + 1; yyy++) {
//									if (xxx < 0 || xxx >= _borderMask.width || yyy < 0 || yyy >= _borderMask.height)
//										continue;
//									
//									if ( borderMaskPixels[yyy * _borderMask.width + xxx] > 0.0f )
//											closestPixelFound = true;
//									
//									if (closestPixelFound) {
//										closestPixelCoords = new Vector2(xxx, yyy);
//										break;
//									}
//								}
//								//if (closestPixelFound)
//								//	break;
//							}
//							
//						} else {
//							closestPixelCoords = new Vector2(xx, yy);
//							break;
//						}
//					}	
//				}*/
//				
//				//if (x == _borderMask.width / 2)
//					//Debug.Log("closestPixelFound: " + closestPixelFound + " closestPixelCoords: " + closestPixelCoords.ToString() );
//				
//				float radius = 0.0f;
//				while (!closestPixelFound) {
//					//totalSearchIterations++;
//					float deltaAngle = 1.0f / radius;
//					
//					for (float angle = 0.0f; angle <= 2.0f * Mathf.PI; angle += deltaAngle) {
//						int xx = x + (int) ( radius * Mathf.Cos(angle) );
//						int yy = y + (int) ( radius * Mathf.Sin(angle) );
//						
//						//Are we within texture bounds?
//						if (xx < 0 || yy < 0 || xx >= _borderMask.width || yy >= _borderMask.height) {
//							continue;
//						}
//					
//						if ( borderMaskPixels[yy * _borderMask.width + xx] >= 1.0f )
//							closestPixelFound = true;
//					
//						if (closestPixelFound) {
//							closestPixelCoords = new Vector2(xx, yy);
//							break;
//						}
//					}
//					
//					if (closestPixelFound)
//						break;
//					
//					//Out of bounds
//					if (radius > _borderMask.width * 1.42f && radius > _borderMask.height * 1.42f) {
//						//Debug.Log("search pixel is out of bounds.");
//						break;
//					}
//						
//					radius += .9f;
//				}
//				
//				if (!closestPixelFound) {
//					Debug.LogError("no closest pixel found.");
//					continue;
//				}
//				
//				float distanceToBorder = ( (new Vector2(x, y) ) - closestPixelCoords).magnitude;
//				
//				if (distanceToBorder <= gradientSize) {
//					float gradientAmount = distanceToBorder / gradientSize;
//					
//					switch (gradientType) {
//					default: case GradientType.linear:
//						gradientAmount = 1.0f - gradientAmount;
//						break;
//						
//					case GradientType.oneMinusSqr:
//						gradientAmount = 1.0f - gradientAmount * gradientAmount;
//						break;
//						
//					case GradientType.sqrOfOneMinusG:
//						gradientAmount = 1.0f - gradientAmount;
//						gradientAmount = gradientAmount * gradientAmount;
//						break;
//					}
//					
//					//gradientAmount = gradientAmount * gradientAmount;
//					
//					resPixels[y * _borderMask.width + x] = gradientAmount;
//				 	
//				}
//					
//			}
//		}
//		
//		//Debug.Log("totalSearchIterations: " + totalSearchIterations);
//		
//		GrayscaleImage resTexture = new GrayscaleImage(_borderMask.width, _borderMask.height, resPixels);
//		
//		return resTexture;
//	}
	
	static public WPGrayscaleImage NormalizeImage(WPGrayscaleImage _texture)
	{
		//System.DateTime startTime = System.DateTime.Now;
		
		//Debug.Log("_texture.format: " + _texture.format.ToString() );
		
		byte[] origPixels = _texture.GetPixels();
		
		byte[] resPixels = _texture.GetPixels();
		
		float maxBrightness = 0.0f;
		float minBrightness = 1.0f;
		
	    //Get max brightness
	    for (int x = 0; x < _texture.width; x++)
	    {
	        for (int y = 0; y < _texture.height; y++)
	        {
				maxBrightness = Mathf.Max(maxBrightness, (float)origPixels[y * _texture.width + x] / 255.0f);
				minBrightness = Mathf.Min(minBrightness, (float)origPixels[y * _texture.width + x] / 255.0f);
	        }
	    }
		
		//Normalize
		for (int x = 0; x < _texture.width; x++)
	    {
	        for (int y = 0; y < _texture.height; y++)
	        {
				resPixels[y * _texture.width + x] = (byte) ( 255.0f * ( ( (float) resPixels[y * _texture.width + x]) / 255.0f - minBrightness ) / (maxBrightness - minBrightness) );
			}
			
		}
		
		WPGrayscaleImage resultTexture = new WPGrayscaleImage(_texture.width, _texture.height, resPixels);
		
	    return resultTexture;
	}
	
	/*static public Texture2D NormalizeImage(Texture2D _texture, ColorChannels channelsToNormalize)
	{
		//System.DateTime startTime = System.DateTime.Now;
		
		//Debug.Log("_texture.format: " + _texture.format.ToString() );
		
		Color[] origPixels = _texture.GetPixels();
		
		Color[] resPixels = _texture.GetPixels();
		
		float maxBrightness = 0.001f;
		float minBrightness = 1.0f;
		
	    //Get max brightness
	    for (int x = 0; x < _texture.width; x++)
	    {
	        for (int y = 0; y < _texture.height; y++)
	        {
				switch (channelsToNormalize) {
				case ColorChannels.r:
					maxBrightness = Mathf.Max(maxBrightness, origPixels[y * _texture.width + x].r);
					minBrightness = Mathf.Min(minBrightness, origPixels[y * _texture.width + x].r);
					break;
					
				case ColorChannels.g:
					maxBrightness = Mathf.Max(maxBrightness, origPixels[y * _texture.width + x].g);
					minBrightness = Mathf.Min(minBrightness, origPixels[y * _texture.width + x].g);
					break;
					
				case ColorChannels.b:
					maxBrightness = Mathf.Max(maxBrightness, origPixels[y * _texture.width + x].b);
					minBrightness = Mathf.Min(minBrightness, origPixels[y * _texture.width + x].b);
					break;
					
				default: case ColorChannels.rgb:
					maxBrightness = Mathf.Max(maxBrightness, (origPixels[y * _texture.width + x].r + origPixels[y * _texture.width + x].g + origPixels[y * _texture.width + x].b) / 3.0f);
					minBrightness = Mathf.Max(minBrightness, (origPixels[y * _texture.width + x].r + origPixels[y * _texture.width + x].g + origPixels[y * _texture.width + x].b) / 3.0f);
					break;
				}
	        }
	    }
		
		//Normalize
		for (int x = 0; x < _texture.width; x++)
	    {
	        for (int y = 0; y < _texture.height; y++)
	        {
				switch (channelsToNormalize) {
				case ColorChannels.r:
					resPixels[y * _texture.width + x].r = (resPixels[y * _texture.width + x].r - minBrightness) / (maxBrightness - minBrightness);
					break;
					
				case ColorChannels.g:
					resPixels[y * _texture.width + x].g = (resPixels[y * _texture.width + x].g - minBrightness) / (maxBrightness - minBrightness);
					break;
					
				case ColorChannels.b:
					resPixels[y * _texture.width + x].b = (resPixels[y * _texture.width + x].b - minBrightness) / (maxBrightness - minBrightness);
					break;
					
				default: case ColorChannels.rgb:
					resPixels[y * _texture.width + x].r = (resPixels[y * _texture.width + x].r - minBrightness) / (maxBrightness - minBrightness);
					resPixels[y * _texture.width + x].g = (resPixels[y * _texture.width + x].g - minBrightness) / (maxBrightness - minBrightness);
					resPixels[y * _texture.width + x].b = (resPixels[y * _texture.width + x].b - minBrightness) / (maxBrightness - minBrightness);
					break;
				}
			}
			
		}
		
		Texture2D resultTexture = new Texture2D(_texture.width, _texture.height, _texture.format, false);
		
		resultTexture.SetPixels( resPixels );
		resultTexture.Apply();
	 
		//Debug.Log("Image blur took " + (System.DateTime.Now - startTime).TotalSeconds + " seconds.");
		
	    return resultTexture;
	}*/
	
	public static Texture2D FlipImage(Texture2D _texture, bool flipX, bool flipY) {
		if (!_texture)
			return null;
		
		Color[] srcPixels = _texture.GetPixels();
		Color[] resPixels = _texture.GetPixels();
		
		for (int x = 0; x < _texture.width; x++) {
			for (int y = 0; y < _texture.height; y++) {
				int flippedX = x;
				int flippedY = y;
				
				if (flipX)
					flippedX = (_texture.width - 1 - x);
				
				if (flipY)
					flippedY = (_texture.height - 1 - y);
					
				resPixels[y * _texture.width + x] = srcPixels[flippedY * _texture.width + flippedX];
			}
		}
		
		Texture2D resTexture = new Texture2D(_texture.width, _texture.height, TextureFormat.RGB24, true);
		resTexture.SetPixels(resPixels);
		resTexture.Apply();
		
		return resTexture;
	}
	
	public static void SaveTextureAsPng(Texture2D _texture, string _path) {
		//Debug.Log("SaveTextureAsPng at path " + _path);
		if (System.IO.File.Exists( _path ) )
			System.IO.File.Delete(_path);
		
		if (_texture == null) {
			Debug.LogError("SaveTextureAsPng: Input texture is null. Aborting.");
			return;
		}
					
		if (_texture.format != TextureFormat.ARGB32 && _texture.format != TextureFormat.RGB24) {
			//Debug.Log("Cannot save the texture as png because of a format mismatch. Texture format: " + _texture.format);
			//Debug.Log("Wrong texture format: " + _texture.format + ". Converting.");
			
			TextureFormat convertToFormat;
			
			if (_texture.format == TextureFormat.DXT1)
				convertToFormat = TextureFormat.RGB24;
			else
				convertToFormat = TextureFormat.ARGB32;
			
			Texture2D convertedTexture = new Texture2D(_texture.width, _texture.height, convertToFormat, false);
			
			Color[] srcPixels = _texture.GetPixels();
			
			convertedTexture.SetPixels( srcPixels );
			convertedTexture.Apply();
			
			_texture = convertedTexture;
		}
		
		//return;
		byte[] bytes = _texture.EncodeToPNG();
		
		//Debug.Log("pathToRefractionMapFile: " + pathToRefractionMapFile);
		FileStream file = File.Open(_path, FileMode.Create);
	    BinaryWriter binary = new BinaryWriter(file);
	    binary.Write(bytes);
	    file.Close();
	
		AssetDatabase.ImportAsset(_path);
		AssetDatabase.Refresh();
	}
		
/*	public static System.Drawing.Image ResizeImage(System.Drawing.Image imgToResize, int _width, int _height)
	{
	   System.Drawing.Bitmap b = new System.Drawing.Bitmap(_width, _height);
	   System.Drawing.Graphics g = System.Drawing.Graphics.FromImage( (System.Drawing.Image)b );
	   g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
	
	   g.DrawImage(imgToResize, 0, 0, _width, _height);
	   g.Dispose();
	
	   return (System.Drawing.Image)b;
	}*/
	
	public static WPGrayscaleImage ResizeImage(WPGrayscaleImage _texture, int _width, int _height, WPFilteringMethod filteringMethod) {
		if (_texture == null) {
			Debug.LogWarning("ResizeImage: input texture is null");
			return null;
		}
		
		//System.DateTime startTime = System.DateTime.Now;
		
		byte[] srcPixels = _texture.GetPixels();
		byte[] dstPixels = new byte[_width * _height];
		
		float xRatio = (float)_texture.width / (float)_width;
		float yRatio = (float)_texture.height / (float)_height;
		
		//200	100
		//xR = 2
		
		//
		//Resize using bilinear interpolation
		for (int x = 0; x < _width; x++) {
			for (int y = 0; y < _height; y++) {
				float xx = x * xRatio;
				float yy = y * yRatio;
					
				//Get neighbour pixels in the original image
				int x0, x1, y0, y1;
				
				//(50,5; 47,5)
				x0 = Mathf.FloorToInt( xx );
				x1 = Mathf.CeilToInt( xx );
				y0 = Mathf.FloorToInt( yy );
				y1 = Mathf.CeilToInt( yy );
				
				//Avoid having the same pixel
				if (x1 == x0)
					x1 = x0 + 1;
				
				if (y1 == y0)
					y1 = y0 + 1;
					
				//Avoid crossing the image borders
				if (x1 < 0)
					x1 = 0;
				
				if (x1 >= _texture.width)
					x1 = _texture.width - 1;
				
				if (y1 < 0)
					y1 = 0;
				
				if (y1 >= _texture.height)
					y1 = _texture.height - 1;
				
				//i = b1 + b2x + b3y + b4xy
				
				//b1 = f(0,0)
				//b2 = f(1,0) - f(0,0)
				//b3 = f(0,1) - f(0,0)
				//b4 = f(0,0) - f(1,0) - f(0,1) + f(1,1)
				
				byte interpolatedColor;
				//if (filteringMethod == FilteringMethod.bilinear) {
					float b1 = (float)srcPixels[y0 * _texture.width + x0];
					float b2 = (float)srcPixels[y0 * _texture.width + x1] - (float)srcPixels[y0 * _texture.width + x0];
					float b3 = (float)srcPixels[y1 * _texture.width + x0] - (float)srcPixels[y0 * _texture.width + x0];
					float b4 = (float)srcPixels[y0 * _texture.width + x0] - (float)srcPixels[y0 * _texture.width + x1] -
								(float)srcPixels[y1 * _texture.width + x0] + (float)srcPixels[y1 * _texture.width + x1];
					
					interpolatedColor = (byte) ( ( b1 + b2 * (xx - (float)x0) + b3 * (yy - (float)y0) + b4 * (xx - (float)x0) * (yy - (float)y0) ) );
				//} else {
					
				//}
					
				dstPixels[y * _width + x] = interpolatedColor;
			}
		}
		
		WPGrayscaleImage resTexture = new WPGrayscaleImage(_width, _height, dstPixels);
		
		//Debug.Log("Successfully resized the texture in " + (System.DateTime.Now - startTime).TotalSeconds + " seconds." );
		
		return resTexture;
	}
	
	public static Texture2D ResizeImage(Texture2D _texture, int _width, int _height)
	{
		if (_texture == null) {
			Debug.LogWarning("ResizeImage: input texture is null");
			return null;
		}
		
		System.DateTime startTime = System.DateTime.Now;
		
		Color[] srcPixels = _texture.GetPixels();
		Color[] dstPixels = new Color[_width * _height];
		
		float xRatio = (float)_texture.width / (float)_width;
		float yRatio = (float)_texture.height / (float)_height;
		
		//200	100
		//xR = 2
		
		//
		//Resize using bilinear interpolation
		for (int x = 0; x < _width; x++) {
			for (int y = 0; y < _height; y++) {
				float xx = x * xRatio;
				float yy = y * yRatio;
					
				//Get neighbour pixels in the original image
				int x0, x1, y0, y1;
				
				//(50,5; 47,5)
				x0 = Mathf.FloorToInt( xx );
				x1 = Mathf.CeilToInt( xx );
				y0 = Mathf.FloorToInt( yy );
				y1 = Mathf.CeilToInt( yy );
				
				//Avoid having the same pixel
				if (x1 == x0)
					x1 = x0 + 1;
				
				if (y1 == y0)
					y1 = y0 + 1;
					
				//Avoid crossing the image borders
				if (x1 < 0)
					x1 = x0 + 1;
				
				if (x1 >= _texture.width)
					x1 = x0 - 1;
				
				if (y1 < 0)
					y1 = y0 + 1;
				
				if (y1 >= _texture.height)
					y1 = y0 - 1;
				
				//i = b1 + b2x + b3y + b4xy
				
				//b1 = f(0,0)
				//b2 = f(1,0) - f(0,0)
				//b3 = f(0,1) - f(0,0)
				//b4 = f(0,0) - f(1,0) - f(0,1) + f(1,1)
				
				Color b1 = srcPixels[y0 * _texture.width + x0];
				Color b2 = srcPixels[y0 * _texture.width + x1] - srcPixels[y0 * _texture.width + x0];
				Color b3 = srcPixels[y1 * _texture.width + x0] - srcPixels[y0 * _texture.width + x0];
				Color b4 = srcPixels[y0 * _texture.width + x0] - srcPixels[y0 * _texture.width + x1] -
							srcPixels[y1 * _texture.width + x0] + srcPixels[y1 * _texture.width + x1];
				
				Color interpolatedColor = b1 + b2 * (xx - (float)x0) + b3 * (yy - (float)y0) + b4 * (xx - (float)x0) * (yy - (float)y0);
				
				dstPixels[y * _width + x] = interpolatedColor;
			}
		}
		
		Texture2D resTexture = new Texture2D(_width, _height, _texture.format, false);
		
		//Debug.Log("format: " + _texture.format);
		
		resTexture.SetPixels( dstPixels );
		resTexture.Apply();
		
		Debug.Log("Successfully resized the texture in " + (System.DateTime.Now - startTime).TotalSeconds + " seconds." );
		
		return resTexture;
	}
	
	public static Vector2 CalculateGeometricalCenter(Texture2D _texture, Color _lookupColor, float _tolerance, WPColorChannels _channel) {
		int totalX = 0, totalY = 0, totalPoints = 0;
		
		Color[] srcPixels = _texture.GetPixels();
		
		for (int x = 0; x < _texture.width; x++) {
			for (int y = 0; y < _texture.height; y++) {
				Color pixelColor = srcPixels[y * _texture.width + x];
				switch (_channel) {
				case WPColorChannels.r:
					if ( Mathf.Abs(pixelColor.r - _lookupColor.r) <= _tolerance ) {
						totalX += x;
						totalY += y;
						totalPoints++;
					}
					break;
					
				case WPColorChannels.g:
					if ( Mathf.Abs(pixelColor.g - _lookupColor.g) <= _tolerance ) {
						totalX += x;
						totalY += y;
						totalPoints++;
					}
					break;
					
				case WPColorChannels.b:
					if ( Mathf.Abs(pixelColor.b - _lookupColor.b) <= _tolerance ) {
						totalX += x;
						totalY += y;
						totalPoints++;
					}
					break;
					
				default: case WPColorChannels.a:
					if ( Mathf.Abs(pixelColor.a - _lookupColor.a) <= _tolerance ) {
						totalX += x;
						totalY += y;
						totalPoints++;
					}
					break;
					
				case WPColorChannels.rgb:
					if ( Mathf.Abs(pixelColor.r - _lookupColor.r) <= _tolerance && Mathf.Abs(pixelColor.g - _lookupColor.g) <= _tolerance &&
								Mathf.Abs(pixelColor.b - _lookupColor.b) <= _tolerance) {
						totalX += x;
						totalY += y;
						totalPoints++;
					}
					break;
				}
			}
		}
		
		if (totalPoints <= 0)
			return new Vector2(_texture.width/2, _texture.height/2);
		
		totalX /= totalPoints;
		totalY /= totalPoints;
		
		Debug.Log("Found geometrical center at" + DimensionsString(totalX, totalY) );
		
		return new Vector2( (float)totalX, (float)totalY );
	}
	
	/*public static Vector2 CalculateGeometricalCenter(GrayscaleImage _texture, float _lookupColor, float _tolerance) {
		int totalX = 0, totalY = 0, totalPoints = 0;
		
		float[] srcPixels = _texture.GetPixels();
		
		for (int x = 0; x < _texture.width; x++) {
			for (int y = 0; y < _texture.height; y++) {
				float pixelColor = srcPixels[y * _texture.width + x];
				
				if ( Mathf.Abs(pixelColor - _lookupColor) <= _tolerance ) {
					totalX += x;
					totalY += y;
					totalPoints++;
				}
			}
		}
		
		if (totalPoints <= 0)
			return new Vector2(_texture.width/2, _texture.height/2);
		
		totalX /= totalPoints;
		totalY /= totalPoints;
		
		//Debug.Log("totalPoints: " + totalPoints);
		Debug.Log("Found geometrical center at" + DimensionsString(totalX, totalY) + "image dimensions:" + DimensionsString(_texture.width, _texture.height) );
		
		return new Vector2( (float)totalX, (float)totalY );
	}*/
	
	public static Texture2D FillChannelWithValue(Texture2D _texture, WPColorChannels _channel, float _value) {
		Color[] srcPixels = _texture.GetPixels();
		
		for (int i = 0; i < _texture.width * _texture.height; i++) {
			switch (_channel) {
			case WPColorChannels.r:
				srcPixels[i].r = _value;
				break;
				
			case WPColorChannels.g:
				srcPixels[i].g = _value;
				break;
				
			case WPColorChannels.b:
				srcPixels[i].b = _value;
				break;
				
			default: case WPColorChannels.a:
				srcPixels[i].a = _value;
				break;
				
			case WPColorChannels.rgb:
				srcPixels[i].r = _value;
				srcPixels[i].g = _value;
				srcPixels[i].b = _value;
				break;
			}
		}
		
		Texture2D resultsTexture = new Texture2D(_texture.width, _texture.height, _texture.format, false);
		resultsTexture.SetPixels(srcPixels);
		resultsTexture.Apply();
		
		return resultsTexture;
	}
	
	/*public static Texture2D FillImage(Texture2D _texture, Color _fillWithColor, Vector2 _fillPos, float _tolerance, ColorChannels _channel) {
		Color[] srcPixels = _texture.GetPixels();
		Color[] resPixels = _texture.GetPixels();
		
		int startX = (int)_fillPos.x;
		int startY = (int)_fillPos.y;
		
		Color startColor = srcPixels[startY * _texture.width + startX];
		
		for (int x = 0; x < _texture.width; x++) {
			for (int y = 0; y < _texture.height; y++) {
				Color pixelColor = srcPixels[y * _texture.width + x];
				switch (_channel) {
				case ColorChannels.r:
					if ( Mathf.Abs(pixelColor.r - _lookupColor.r) <= _tolerance ) {
						
					}
					break;
					
				case ColorChannels.g:
					if ( Mathf.Abs(pixelColor.g - _lookupColor.g) <= _tolerance ) {
						
					}
					break;
					
				case ColorChannels.b:
					if ( Mathf.Abs(pixelColor.b - _lookupColor.b) <= _tolerance ) {
						
					}
					break;
					
				default: case ColorChannels.a:
					if ( Mathf.Abs(pixelColor.a - _lookupColor.a) <= _tolerance ) {
						
					}
					break;
					
				case ColorChannels.rgb:
					if ( Mathf.Abs(pixelColor.r - _lookupColor.r) <= _tolerance && Mathf.Abs(pixelColor.g - _lookupColor.g) <= _tolerance &&
								Mathf.Abs(pixelColor.b - _lookupColor.b) <= _tolerance) {
						
					}
					break;
				}
			}
		}
	}*/
	
	public static void SetTextureImporterFormat(Texture2D _texture, TextureImporterFormat _format, bool _isReadable) {
		string assetPath = AssetDatabase.GetAssetPath( _texture );
		TextureImporter tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
        if( tImporter != null ) {
			tImporter.textureType = TextureImporterType.Advanced;
            tImporter.textureFormat = _format;
			tImporter.isReadable = _isReadable;
			
            AssetDatabase.ImportAsset( assetPath );
			AssetDatabase.Refresh();
        }
	}
	
	public static void MakeTexturesReadable(Texture2D[] _textures, bool isReadable) {
		if (null == _textures) {
			Debug.LogError("null == _textures");
			return;
		}
		
		foreach (Texture2D tex in _textures) {
			if (null == tex)
				continue;
			
			MakeTextureReadable(tex, isReadable);
		}
	}
	
	public static void MakeTextureReadable(Texture2D _texture, bool isReadable) {
		if (!_texture)
			return;
		
		TextureImporter tImporter = AssetImporter.GetAtPath( AssetDatabase.GetAssetPath(_texture) ) as TextureImporter;
        if( tImporter != null ) {
            tImporter.isReadable = isReadable;
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_texture));                
        }
	}
	
	public static void CompressTextures(Texture2D[] _textures, bool isCompressed) {
		foreach (Texture2D tex in _textures) {
			if (null == tex)
				continue;
			
			CompressTexture(tex, isCompressed);
		}
	}
	
	public static void CompressTexture(Texture2D _texture, bool isCompressed) {
		if (!_texture)
			return;
		
		TextureImporter tImporter = AssetImporter.GetAtPath( AssetDatabase.GetAssetPath(_texture) ) as TextureImporter;
        if( tImporter != null ) {
            if (isCompressed)
				tImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
			else
				tImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_texture));                
        }
	}
	
	public static string SaveTextureAsPngAtAssetPath(Texture2D _texture, string _assetPath, bool shouldDeleteOriginal) {
		string oldFilePath = AssetPathToFilePath(_assetPath);
		//Debug.Log("oldFilePath: " + oldFilePath);
		
		string newAssetPath = System.IO.Path.ChangeExtension(_assetPath, ".png");
		//Debug.Log("newAssetPath: " + newAssetPath);
		
		string newFilePath = AssetPathToFilePath(newAssetPath);
		//Debug.Log("newFilePath: " + newFilePath);
		
		WPHelper.SaveTextureAsPng(_texture, newFilePath);
		
		if (shouldDeleteOriginal) {
			//Debug.Log("Extension: " + System.IO.Path.GetExtension(oldFilePath) );
			if ( System.IO.Path.GetExtension(oldFilePath) != ".png" ) {
				Debug.Log("Extension is not png, deleting older file.");
				AssetDatabase.DeleteAsset(_assetPath);
			} else {
				//Debug.Log("Extension is png.");	
			}
		}
			
		return newAssetPath;
	}
	
	public static string AssetPathToFilePath(string _assetPath) {
		return Application.dataPath + "/" + _assetPath.Remove( _assetPath.IndexOf("Assets/"), "Assets/".Length);
	}
	
	public static string FilePathToAssetPath(string _filePath) {
		return _filePath.Substring(_filePath.LastIndexOf("Assets/"));
	}
	
	public static int GetNearestPOT(int _number) {
		_number--;
		_number |= _number >> 1;
		_number |= _number >> 2;
		_number |= _number >> 4;
		_number |= _number >> 8;
		_number |= _number >> 16;
		_number++;
		
		return _number;
	}
	
	public static void CreateWaterSystemDirs() {
		//Debug.Log("CreateWaterSystemDirs");
		//System.IO.Directory.CreateDirectory(Application.dataPath + "/" + waterSystemPath);
		System.IO.Directory.CreateDirectory( AssetPathToFilePath(waterSystemPath) + "Temp/");
	}
	
	public static void CleanupTempFiles() {
		if (!Directory.Exists( AssetPathToFilePath(waterSystemPath) + "Temp/" ) )
			return;
		System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(AssetPathToFilePath(waterSystemPath) + "Temp/");
		foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
	}
	
	public static bool HasSuffix(string _path, string _suffix) {
		//string filenameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension( _path );
		int suffixIndex = _path.LastIndexOf( _suffix );
		
		if (suffixIndex == -1)
			return false;
		
		return true;
	}
	
	public static string RemoveSuffixFromFilename(string _path, string _suffix) {
		string directory = Path.GetDirectoryName(_path);
		string filenameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension( _path );
		string extension = System.IO.Path.GetExtension( _path );
		
		int suffixIndex = filenameWithoutExtension.LastIndexOf( _suffix );
		
		if (suffixIndex == -1) {
			Debug.LogError("suffix " + _suffix + " not found for path " + _path);
			return null;
		}
		
		filenameWithoutExtension = filenameWithoutExtension.Remove( suffixIndex, _suffix.Length);
		
		//Debug.Log("suffixIndex: " + suffixIndex + " length: " + _suffix.Length + " filenameWithoutExtension: " + filenameWithoutExtension);
		
		return directory + "/" + filenameWithoutExtension + extension;
	}
	
	public static string AddSuffixToFilename(string _path, string _suffix) {
		//Debug.Log("AddSuffixToFilename: " + _path + " suffix: " + _suffix);
		string directory = Path.GetDirectoryName(_path);
		string filenameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension( _path );
		string extension = System.IO.Path.GetExtension( _path );
		//Debug.Log("directory: " + directory + " filenameWithoutExtension: " + filenameWithoutExtension + " extension: " + extension);
		//Debug.Log("new path: " + directory + "/" + filenameWithoutExtension + _suffix + extension);
		return directory + "/" + filenameWithoutExtension + _suffix + extension;
	}
	
	public static string DimensionsString(int x, int y) {
		return " (" + x + "; " + y + ") ";
	}
	
	public static string DimensionsString(float x, float y) {
		return " (" + x + "; " + y + ") ";
	}
	
	private static string progressLog = "";
	
	public static void LogToProgressFile(string _stringToLog) {
		string progressFilePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\Water_Progress.txt";
		
		//Debug.Log("progressFilePath: " + progressFilePath);
		
		/*if ( !File.Exists(progressFilePath) ) {
			Debug.Log("creating text file at path " + progressFilePath);
			File.CreateText( progressFilePath );
		}*/
		
		progressLog += System.DateTime.Now.ToShortTimeString() + ": " + _stringToLog  + System.Environment.NewLine;
		
		bool wasException = false;
		
		try {
			File.AppendAllText(progressFilePath, progressLog);
		} catch {
			wasException = true;
		}
		
		if (!wasException)
			progressLog = "";
		
		/*string text = "";
		if ( File.Exists( progressFilePath ) )
			text = File.ReadAllText(progressFilePath);
		
        File.WriteAllText(progressFilePath, text + _stringToLog + System.Environment.NewLine);*/
	}
	
	public static void DeleteProgressFile() {
		string progressFilePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\Water_Progress.txt";
		
		if ( File.Exists( progressFilePath ) )
			File.Delete( progressFilePath );
		
	}
		
	/*static public GameObject FindTerrain()
	{
		GameObject terrainGameObject;
		T4MObj tempTerrain = (T4MObj)FindObjectOfType( typeof(T4MObj) );
		if (tempTerrain != null)
			terrainGameObject = tempTerrain.gameObject;
		else
			terrainGameObject = ( (Terrain)FindObjectOfType( typeof(Terrain) ) ).gameObject;	
		return terrainGameObject;
	}*/
}
		                
/*public class HelperGameObject : MonoBehaviour {
	public Collider terrainCollider = null;
	
	void Awake() {
		DontDestroyOnLoad( gameObject );
	}
	
	void OnLevelWasLoaded() {
		Debug.Log(this + " OnLevelWasLoaded");
		
	}
			
}*/
//#endif