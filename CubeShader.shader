Shader "Unlit/CubeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scalar ("Scalar", float) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM


            #define MAX_STEPS 100
            #define MAX_DIST  1000
            #define SURF_DIST 1e-3


            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro 	: TEXTCOORD1;
                float3 hit	: TEXTCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Scalar;

            float sdCube(float3 p, float3 c, float3 s)
            {
            	return length(max(abs(p - c) - s, 0.0));
            }
            
            float sdSphere(float3 p, float3 c, float r)
            {
            	return length(p - c) - r;
            }
            

            float sdCapsule(float3 p, float3 a, float3 b, float r)
            {
                float3 ab = b - a;		float3 ap = p -a;
                float t = dot(ab, ap) / dot(ab, ab);
                t = clamp(t, 0, 1.0);
                
                float3 c = a + ab * t;
                
                return length(p - c) - r;
            }		
            

            float GetDist(float3 p)
            {
                float cubo = sdCube(p, float3(-0.2, -0.2, -0.2), float3(0.2, 0.2, 0.2));
                
                float e = sdSphere(p, float3(0, 0.1, 0), 0.2);
                float b = sdSphere(p, float3(0, -0.2, 0), 0.2);
                
                float c = sdCapsule(p, float3(-0.2, 0.2, 0.1), float3(0.2, 0.2, 0.1), 0.1);
                
                
                cubo = max(cubo, e);
                
    //	     	return cubo;
                
                e = max(-e, b);
	     	    return min(e, c);		//sdSphere(p, float3(0, 0, 0), 0.2);
            }

            float RayMarch(float3 ro, float3 rd)
            {
                float dor = 0;			float ds;
                
                for (int i = 0; i < MAX_STEPS; i++)
                {
                    float3 p = ro + dor * rd;
                    
                    ds = GetDist(p);		dor += ds;
                    
                    if (ds < SURF_DIST || dor > MAX_DIST)
                    {
                        break;
                    }
                }
                
                return dor;
            }
		

	        float3 GetNormal(float3 p)
	        {
	     	    float2 e = float2(1e-2, 0);
	     	
	     	    float3 n = GetDist(p) - float3(
	     	    GetDist(p - e.xyy), 
	     	    GetDist(p - e.yxy), 
	     	    GetDist(p - e.yyx));
	     	
	     	    return normalize(n);
	        }


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);


		        o.ro = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
                o.hit = v.vertex;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = fixed4(0, 1, 0, 1);		//tex2D(_MainTex, i.uv);
//                fixed4 col = length(i.uv - 0.5);       //tex2D(_MainTex, i.uv);
//                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
//                if (i.uv.x == i.uv.y)
//                    col = 0;

            	float3 ro = i.ro;	float3 rd = normalize(i.hit - i.ro);

                float d = RayMarch(ro, rd);
                
                if (d < MAX_DIST)
                {
                    float3 p = ro + rd * d;
                    col.rgb  = GetNormal(p);
                }
                else discard;


// 			    col.rgb  = GetNormal(p);

                return col;
            }
            ENDCG
        }
    }
}
