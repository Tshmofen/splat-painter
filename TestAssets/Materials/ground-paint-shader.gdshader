shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 albedo : source_color = vec4(1,1,1,1);
uniform vec4 roughness = vec4(1,1,1,1);
uniform vec4 specular = vec4(0.5,0.5,0.5,0.5);
uniform vec4 metallic = vec4(0,0,0,0);
uniform vec4 normal_scale = vec4(1,1,1,1);

uniform sampler2D texture_splatmap : filter_nearest_mipmap_anisotropic;

uniform sampler2D texture1_albedo_metallic : source_color,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture1_normal_roughness : hint_roughness_normal,filter_nearest_mipmap,repeat_enable;

uniform sampler2D texture2_albedo_metallic : source_color,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture2_normal_roughness : hint_roughness_normal,filter_nearest_mipmap,repeat_enable;

uniform sampler2D texture3_albedo_metallic : source_color,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture3_normal_roughness : hint_roughness_normal,filter_nearest_mipmap,repeat_enable;

uniform sampler2D texture4_albedo_metallic : source_color,filter_nearest_mipmap,repeat_enable;
uniform sampler2D texture4_normal_roughness : hint_roughness_normal,filter_nearest_mipmap,repeat_enable;

uniform vec3 uv_scale = vec3(1,1,1);
uniform vec3 uv_offset = vec3(0,0,0);

float max3(float r, float g, float b) {
	return max(r, max(g, b));
}

vec4 get_blended_weights(vec4 splat, vec4 bumps) {
	float delta = 0.2;
	vec4 height = bumps + splat;
	
	// Mitigation: nullify layers with near-zero splat
	height *= smoothstep(0, 0.05, splat);

	vec4 weight = height + delta;
	weight.r -= max3(height.g, height.b, height.a);
	weight.g -= max3(height.r, height.b, height.a);
	weight.b -= max3(height.g, height.r, height.a);
	weight.a -= max3(height.g, height.b, height.r);

	return clamp(weight, 0, 1);
}

void fragment() {
	vec2 splat_uv = UV;
	vec2 base_uv = UV * uv_scale.xy + uv_offset.xy;
	
	vec4 albedo1 = texture(texture1_albedo_metallic,base_uv);
	vec4 normal1 = texture(texture1_normal_roughness,base_uv);
	float roughness1 = albedo1.a * roughness.r;
	float metallic1 = albedo1.a * metallic.r;
	
	vec4 albedo2 = texture(texture2_albedo_metallic,base_uv);
	vec4 normal2 = texture(texture2_normal_roughness,base_uv);
	float roughness2 = normal2.a * roughness.g;
	float metallic2 = albedo2.a * metallic.g;
	
	vec4 albedo3 = texture(texture3_albedo_metallic,base_uv);
	vec4 normal3 = texture(texture3_normal_roughness,base_uv);
	float roughness3 = normal3.a * roughness.b;
	float metallic3 = albedo3.a * metallic.b;
	
	vec4 albedo4 = texture(texture4_albedo_metallic,base_uv);
	vec4 normal4 = texture(texture4_normal_roughness,base_uv);
	float roughness4 = normal4.a * roughness.a;
	float metallic4 = albedo4.a * metallic.a;
	
	vec4 splat = normalize(texture(texture_splatmap,splat_uv));
	vec4 weight = get_blended_weights(splat, vec4(albedo1.a,albedo2.a,albedo3.a,albedo4.a));
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
