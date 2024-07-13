shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 albedo : source_color = vec4(1,1,1,1);
uniform vec4 roughness = vec4(1,1,1,1);
uniform vec4 specular = vec4(0.5,0.5,0.5,0.5);
uniform vec4 metallic = vec4(0,0,0,0);
uniform vec4 normal_scale = vec4(1,1,1,1);

uniform sampler2D texture_splatmap : filter_nearest;

uniform sampler2D texture1_albedo : source_color,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture1_normal :hint_roughness_normal,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture1_roughness : hint_roughness_a,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture1_metallic : hint_default_white,filter_nearest_mipmap,repeat_enable;

uniform sampler2D texture2_albedo : source_color,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture2_normal :hint_roughness_normal,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture2_roughness : hint_roughness_a,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture2_metallic : hint_default_white,filter_nearest_mipmap,repeat_enable;

uniform sampler2D texture3_albedo : source_color,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture3_normal :hint_roughness_normal,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture3_roughness : hint_roughness_a,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture3_metallic : hint_default_white,filter_nearest_mipmap,repeat_enable;

uniform sampler2D texture4_albedo : source_color,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture4_normal :hint_roughness_normal,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture4_roughness : hint_roughness_a,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture4_metallic : hint_default_white,filter_nearest_mipmap,repeat_enable;

uniform vec3 uv1_scale = vec3(1,1,1);
uniform vec3 uv1_offset = vec3(0,0,0);
uniform vec3 uv2_scale = vec3(1,1,1);
uniform vec3 uv2_offset = vec3(0,0,0);

float max_custom(float r, float g, float b) {
	return max(r, max(g, b));
}

vec4 get_blended_weights(vec4 splat) {
	// we consider black as 4th texture, alpha is too unconvenient
	vec4 weight = smoothstep(0, 0.05, splat);
	weight.a = 1.0 - max(max(weight.r, weight.g), weight.b);
	
	return weight;
}

void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;
}

void fragment() {
	vec4 roughness_texture_channel = vec4(0,0,0,1);
	vec4 metallic_texture_channel = vec4(0,0,0,1);
	vec2 base_uv = UV;
	
	vec4 albedo1 = texture(texture1_albedo,base_uv);
	float metallic1 = dot(texture(texture1_metallic,base_uv),metallic_texture_channel) * metallic.r;
	float roughness1 = dot(texture(texture1_roughness,base_uv),roughness_texture_channel) * roughness.r;
	vec4 normal1 = texture(texture1_normal,base_uv);
	
	vec4 albedo2 = texture(texture2_albedo,base_uv);
	float metallic2 = dot(texture(texture2_metallic,base_uv),metallic_texture_channel) * metallic.g;
	float roughness2 = dot(texture(texture2_roughness,base_uv),roughness_texture_channel) * roughness.g;
	vec4 normal2 = texture(texture2_normal,base_uv);
	
	vec4 albedo3 = texture(texture3_albedo,base_uv);
	float metallic3 = dot(texture(texture3_metallic,base_uv),metallic_texture_channel) * metallic.b;
	float roughness3 = dot(texture(texture3_roughness,base_uv),roughness_texture_channel) * roughness.b;
	vec4 normal3 = texture(texture3_normal,base_uv);
	
	vec4 albedo4 = texture(texture4_albedo,base_uv);
	float metallic4 = dot(texture(texture4_metallic,base_uv),metallic_texture_channel) * metallic.a;
	float roughness4 = dot(texture(texture4_roughness,base_uv),roughness_texture_channel) * roughness.a;
	vec4 normal4 = texture(texture4_normal,base_uv);
	
	vec4 splat = normalize(texture(texture_splatmap,base_uv));
	vec4 weight = get_blended_weights(splat);
	float weight_sum = (weight.r + weight.g + weight.b + weight.a);
	
	ALBEDO = albedo.rgb * (
		weight.r * albedo1.rgb +
		weight.g * albedo2.rgb +
		weight.b * albedo3.rgb +
		weight.a * albedo4.rgb
	) / weight_sum;
	
	ROUGHNESS = (
		weight.r * roughness1 +
		weight.g * roughness2 +
		weight.b * roughness3 +
		weight.a * roughness4
	) / weight_sum;
	
	METALLIC = (
		weight.r * metallic1 +
		weight.g * metallic2 +
		weight.b * metallic3 +
		weight.a * metallic4
	) / weight_sum;
	
	NORMAL_MAP = (
		weight.r * normal1.rgb +
		weight.g * normal2.rgb +
		weight.b * normal3.rgb +
		weight.a * normal4.rgb
	) / weight_sum;
	
	NORMAL_MAP_DEPTH = (
		weight.r * normal_scale.r +
		weight.g * normal_scale.g +
		weight.b * normal_scale.b +
		weight.a * normal_scale.a
	) / weight_sum;
	
	SPECULAR = (
		weight.r * specular.r +
		weight.g * specular.g +
		weight.b * specular.b +
		weight.a * specular.a
	) / weight_sum;
}
