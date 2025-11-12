package com.omniscape.secure;

import android.content.Context;
import android.content.SharedPreferences;
import androidx.security.crypto.EncryptedSharedPreferences;
import androidx.security.crypto.MasterKey;

public class OmniSecure {
    private static SharedPreferences prefs;

    public static void init(Context ctx) throws Exception {
        MasterKey key = new MasterKey.Builder(ctx)
                .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
                .build();
        prefs = EncryptedSharedPreferences.create(
                ctx,
                "omni_secure_prefs",
                key,
                EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
                EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM);
    }

    public static void set(String k, String v) { prefs.edit().putString(k, v).apply(); }
    public static String get(String k) { return prefs.getString(k, null); }
    public static void del(String k) { prefs.edit().remove(k).apply(); }
}
