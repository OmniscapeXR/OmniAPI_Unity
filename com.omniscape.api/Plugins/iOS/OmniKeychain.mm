#import <Security/Security.h>

static NSString* ns(const char* s) { return s ? [NSString stringWithUTF8String:s] : @""; }

extern "C" {
void kc_set(const char* service, const char* key, const char* value) {
    NSString* s = ns(service), *k = ns(key), *v = ns(value);
    NSData* data = [v dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary* query = @{(__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
                            (__bridge id)kSecAttrService: s,
                            (__bridge id)kSecAttrAccount: k};
    SecItemDelete((__bridge CFDictionaryRef)query);
    NSDictionary* attrs = @{(__bridge id)kSecValueData: data};
    NSMutableDictionary* add = [query mutableCopy];
    [add addEntriesFromDictionary:attrs];
    SecItemAdd((__bridge CFDictionaryRef)add, NULL);
}

const char* kc_get(const char* service, const char* key) {
    NSString* s = ns(service), *k = ns(key);
    NSDictionary* query = @{(__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
                            (__bridge id)kSecAttrService: s,
                            (__bridge id)kSecAttrAccount: k,
                            (__bridge id)kSecReturnData: @YES};
    CFTypeRef result = NULL;
    if (SecItemCopyMatching((__bridge CFDictionaryRef)query, &result) == errSecSuccess) {
        NSData* data = (__bridge_transfer NSData*)result;
        NSString* val = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
        return strdup([val UTF8String]);
    }
    return strdup("");
}

void kc_del(const char* service, const char* key) {
    NSString* s = ns(service), *k = ns(key);
    NSDictionary* query = @{(__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
                            (__bridge id)kSecAttrService: s,
                            (__bridge id)kSecAttrAccount: k};
    SecItemDelete((__bridge CFDictionaryRef)query);
}
}
