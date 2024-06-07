<template>
  <div style="border: white dotted 3px; width: 100%; position:relative;">
    <div style="width:28%; border: red solid 3px;">
      <ul>
        <li v-for="(source, index) in sources" :key="index" @click="setUrl(source)" :className="isUrlActive(source) ? 'active' : ''">{{ source }}</li>
      </ul>
      <CriteriaInput :changeSpeciality="changeSpeciality" :changeArea="changeArea"></CriteriaInput>
      <button @click="findJobs()">Find Job</button>
    </div>

    <div style="position: absolute; top:5%; left: 30%; border: red dashed 3px; width:70%;">
      <h3 v-if="loading">Loading...</h3>

      <a v-for="(el, index) in jobs" :key="index" :href="el['link']" target="_blank" style="width:48%">
        {{el['title']}}
      </a>
    </div>
  </div>
</template>

<script>
import CriteriaInput from './components/CriteriaInput.vue'

export default {
components: {
  CriteriaInput
},

  data() {
    return {
      sources: [
        'linkedIn',
        'rabotaBy'
      ],
      jobs: [],
      urls: [
        {
          name: 'linkedIn',
          path: 'https://www.linkedin.com/search/results/all/?',
          active: false
        },
        {
          name: 'rabotaBy',
          path: 'https://rabota.by/search/vacancy/?',
          active: false
        }
      ],
      speciality: '',
      area: '',
      loading: false
    }
  },

  methods: {
    async findJobs() {
      var loadedJobs = [];
      this.jobs = [];
      
      this.urls.forEach(async url => {
        if(url.active)
        {
          this.loading = true;

          loadedJobs = await fetch(
            'https://localhost:7150/Jobs/GetList',
            {
              body: JSON.stringify({
                url: url.path,
                speciality: this.speciality,
                area: this.area,
                source: url.name
              }),
              method: 'POST',
              headers: {
                'Content-Type' : 'application/json'            
              }
            }
          ).then(response => response.json());

          loadedJobs.forEach(job => this.jobs.push(job));          
        }

        this.loading = false
      });
    },

    changeSpeciality(value) {
      this.speciality = String(value).trim();
    },

    changeArea(value) {
      this.area = String(value).trim();
    },

    setUrl(value) {
      this.urls.forEach(url => {
        if (url['name'] === value) {
          url['active'] = !url['active'];
          return;
        }
      })
    },

    isUrlActive(value) {
      const active = this.urls.find(x => x['name'] == value)['active'];      
      return active;
    }
  }  
}
</script>

<style scoped>
  .root-container {
    display: inline;
  }

  a {
      display: block;
      text-decoration:none;
      color:black;
      padding: 3px;
      margin: 2px;
      background-color: gray;
  }

  a:hover {
    color:lightgray;
    background: #333;
  }

  a:visited {
    color:darkgray;
    font-style: italic;
  }

  h3 {
    color:white;
    font-weight: bold;
  }

  ul {
        list-style: none;
        width: 200px;
        padding: 0;
        border-radius: 3px;
        border: 3px solid #000
    }

    li {
        display: block;
        background: #1A032D;
        color: #fff;
        padding: 20px 0;
        text-align: center;
    }   

    li:hover, li.active {
        background: #24043e;
        cursor: pointer;
    }
</style>
