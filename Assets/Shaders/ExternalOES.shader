Shader "Custom/ExternalOES"
{

    //converts to linear color space
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Geometry" }
        Pass
        {
            GLSLPROGRAM
    
            //get rid of black bar? and flip vertically
            vec4 _UvTopLeftRight = vec4(0, .99, .99, .99);
            vec4 _UvBottomLeftRight = vec4(0 , 0, .99, 0);

            #ifdef VERTEX
 
            varying vec2 textureCoord;
 
            void main()
            {
                vec2 uvTop = mix(_UvTopLeftRight.xy, _UvTopLeftRight.zw, gl_MultiTexCoord0.x);
                vec2 uvBottom = mix(_UvBottomLeftRight.xy, _UvBottomLeftRight.zw, gl_MultiTexCoord0.x);
                textureCoord = mix(uvTop, uvBottom, gl_MultiTexCoord0.y);

                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
            }
            #endif
 
            #ifdef FRAGMENT
            #extension GL_OES_EGL_image_external_essl3 : enable
            uniform samplerExternalOES _MainTex;
            varying vec2 textureCoord;
            float _Gamma = 2.2;

            void main()
            {
                vec4 color = texture(_MainTex, textureCoord);
 
                color.rgb = pow(color.rgb, vec3(_Gamma, _Gamma, _Gamma));
                color.rgb = clamp(color.rgb, 0.0, 0.996);
 
                gl_FragColor = color;
            }
            #endif
            ENDGLSL
        }
    }
}
