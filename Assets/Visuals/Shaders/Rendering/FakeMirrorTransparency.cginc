#ifndef FAKE_MIRROR_TRANSPARENCY_INCLUDED

    #define FAKE_MIRROR_TRANSPARENCY_INCLUDED

    #if FAKE_MIRROR_TRANSPARENCY
        half _FakeMirrorTransparencyMultiplier;
        half _FakeMirrorTransparency;
    #endif

    #if FAKE_MIRROR_TRANSPARENCY

        #define APPLY_FAKE_MIRROR_TRANSPARENCY(c) c = FakeMirrorTransparency(c);
        #define APPLY_FAKE_MIRROR_TRANSPARENCY_SQUARED(c) c = FakeMirrorTransparencySquared(c);

        inline half4 FakeMirrorTransparency(half4 c) {
            const half alpha = _FakeMirrorTransparencyMultiplier * _FakeMirrorTransparency;
            c.rgb *= alpha;
            c.a = alpha;
            return c;
        }

        inline half4 FakeMirrorTransparencySquared(half4 c) {
            const half alpha = _FakeMirrorTransparencyMultiplier * _FakeMirrorTransparency * _FakeMirrorTransparency;
            c.a = alpha;
            c.rgb *= c.a;
            //c.rgb *= alpha;
            //c.a = alpha;
            return c;
        }

        inline half4 GetFakeMirrorAlpha() {
            return _FakeMirrorTransparencyMultiplier * _FakeMirrorTransparency;
        }

        inline half4 GetFakeMirrorAlphaSquared() {
            return _FakeMirrorTransparencyMultiplier * _FakeMirrorTransparency * _FakeMirrorTransparency;
        }

    #else
        #define APPLY_FAKE_MIRROR_TRANSPARENCY(c)
        #define APPLY_FAKE_MIRROR_TRANSPARENCY_SQUARED(c)
    #endif

#endif
