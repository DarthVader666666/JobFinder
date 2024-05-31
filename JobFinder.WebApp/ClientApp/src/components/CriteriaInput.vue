<template>
    <div>
        <input @input="changeSpeciality($event.target.value)" type="text" placeholder="Enter spiciality">
        <input @input="changeArea($event.target.value)" type="text" placeholder="Enter area">
    </div>
</template>
<script>
export default {
props: {
    changeSpeciality: {
        type: Function,
        required: true
    },
    changeArea: {
        type: Function,
        required: true
    }
}

}
</script>